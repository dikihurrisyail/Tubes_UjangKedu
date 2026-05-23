using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// ============================================================
/// UjangKeduAduh
/// ============================================================
///
/// STRATEGI:
/// Bot agresif yang:
/// 1. Mengejar musuh tanpa henti
/// 2. Menabrak musuh
/// 3. Menembak dengan firepower maksimum
///
/// GREEDY STRATEGY:
/// - Selalu memilih aksi paling agresif
/// - Selalu mendekati musuh
/// - Selalu memakai firepower maksimum
/// - Saat tabrakan -> spam peluru besar
///
/// OBJECTIVE FUNCTION:
/// - Maksimalkan bullet damage
/// - Maksimalkan ram damage
/// - Menekan musuh secepat mungkin
///
/// Cocok untuk:
/// - Duel 1v1
/// - Arena kecil
/// - Melawan bot defensif
/// ============================================================
/// </summary>
public class UjangKeduAduh : Bot
{
    // =========================================================
    // DATA MUSUH
    // =========================================================
    double enemyX;
    double enemyY;

    bool enemyDetected = false;

    // Arah gerakan
    int moveDir = 1;

    Random rnd = new Random();

    static void Main(string[] args)
    {
        new UjangKeduAduh().Start();
    }

    UjangKeduAduh()
        : base(BotInfo.FromFile("UjangKeduAduh.json")) { }

    public override void Run()
    {
        // =====================================================
        // WARNA BOT
        // =====================================================
        BodyColor = Color.DarkRed;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.OrangeRed;
        ScanColor = Color.White;
        TracksColor = Color.Black;
        GunColor = Color.Firebrick;

        // =====================================================
        // RADAR & GUN INDEPENDENT
        // =====================================================
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning)
        {
            // =================================================
            // RADAR SWEEP
            // =================================================
            // Radar terus berputar mencari musuh
            SetTurnRadarRight(360);

            // =================================================
            // JIKA MUSUH TERDETEKSI
            // =================================================
            if (enemyDetected)
            {
                // =============================================
                // KEJAR MUSUH
                // =============================================
                // Greedy:
                // Selalu mendekati target secepat mungkin
                double angleToEnemy =
                    DirectionTo(enemyX, enemyY);

                double bodyTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - Direction);

                SetTurnLeft(bodyTurn);

                // =============================================
                // MAJU TABRAK MUSUH
                // =============================================
                // Fokus utama: ram musuh
                SetForward(200);

                // =============================================
                // AIM KE MUSUH
                // =============================================
                double gunTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - GunDirection);

                SetTurnGunLeft(gunTurn);

                // =============================================
                // TEMBAK FULL POWER
                // =============================================
                // Firepower maksimum = 3
                if (Math.Abs(gunTurn) < 15 &&
                    GunHeat == 0)
                {
                    SetFire(3);
                }
            }
            else
            {
                // =================================================
                // SEARCH MODE
                // =================================================
                // Jika belum ada musuh:
                // muter sambil maju
                SetTurnLeft(25);
                SetForward(100);
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // =====================================================
        // UPDATE POSISI MUSUH
        // =====================================================
        enemyX = e.X;
        enemyY = e.Y;

        enemyDetected = true;

        // =====================================================
        // RADAR LOCK
        // =====================================================
        // Radar langsung fokus ke musuh
        double radarTurn =
            NormalizeRelativeAngle(
                DirectionTo(enemyX, enemyY)
                - RadarDirection);

        SetTurnRadarLeft(radarTurn);

        // =====================================================
        // TEMBAK LANGSUNG SAAT SCAN
        // =====================================================
        if (GunHeat == 0)
            SetFire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // =====================================================
        // CLOSE COMBAT MODE
        // =====================================================
        // Saat tabrakan:
        // spam peluru maksimum
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);

        // Dorong terus musuh
        SetForward(150);

        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =====================================================
        // ANTI STUCK WALL
        // =====================================================
        // Jika nabrak tembok:
        // mundur lalu cari lagi
        SetBack(100);

        SetTurnLeft(60 + rnd.Next(40));

        moveDir *= -1;

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // =====================================================
        // TETAP AGRESIF SAAT DITEMBAK
        // =====================================================
        // Tidak dodge,
        // malah makin maju
        SetForward(120);

        // Kadang belok sedikit
        if (rnd.Next(100) < 30)
            SetTurnLeft(rnd.Next(-25, 25));

        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // =====================================================
        // RESET TARGET
        // =====================================================
        enemyDetected = false;

        // Cari musuh baru
        SetTurnRadarRight(360);
    }
}