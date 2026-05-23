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
/// - Mengincar musuh paling dekat
/// - Mengejar lalu menabrak musuh
/// - Menembak hanya saat jarak dekat
/// - Anti nyangkut tembok
///
/// GREEDY STRATEGY:
/// Selalu memilih target dengan jarak terdekat
/// karena:
/// - peluang tabrak lebih tinggi
/// - peluang hit lebih besar
/// - damage lebih konsisten
///
/// OBJECTIVE:
/// - Maksimalkan ram damage
/// - Maksimalkan hit accuracy
/// - Maksimalkan pressure agresif
/// ============================================================
/// </summary>
public class UjangKeduAduh : Bot
{
    // =========================================================
    // DATA TARGET
    // =========================================================
    double enemyX;
    double enemyY;
    double enemyDistance = double.MaxValue;

    bool enemyDetected = false;

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
            SetTurnRadarRight(45);

            // =================================================
            // ANTI WALL
            // =================================================
            double margin = 120;

            bool nearWall =
                X < margin ||
                X > ArenaWidth - margin ||
                Y < margin ||
                Y > ArenaHeight - margin;

            if (nearWall)
            {
                // Kembali ke tengah arena
                double centerAngle =
                    NormalizeRelativeAngle(
                        DirectionTo(
                            ArenaWidth / 2,
                            ArenaHeight / 2)
                        - Direction);

                SetTurnLeft(centerAngle);

                SetForward(220);

                Go();

                continue;
            }

            // =================================================
            // JIKA TARGET ADA
            // =================================================
            if (enemyDetected)
            {
                // =============================================
                // KEJAR TARGET TERDEKAT
                // =============================================
                double angleToEnemy =
                    DirectionTo(enemyX, enemyY);

                double bodyTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - Direction);

                // Body langsung hadap musuh
                SetTurnLeft(bodyTurn);

                // Maju agresif
                if (enemyDistance > 150)
                    SetForward(200);
                else
                    SetForward(120);

                // =============================================
                // AIM GUN
                // =============================================
                double gunTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - GunDirection);

                SetTurnGunLeft(gunTurn);

                // =============================================
                // TEMBAK HANYA JIKA SUDAH DEKAT
                // =============================================
                // Menghemat peluru
                // dan meningkatkan akurasi
                if (enemyDistance < 170 &&
                    Math.Abs(gunTurn) < 12 &&
                    GunHeat == 0 &&
                    Energy > 1)
                {
                    SetFire(3);
                }
            }
            else
            {
                // =================================================
                // SEARCH MODE
                // =================================================
                SetTurnLeft(20);

                SetForward(100);
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // =====================================================
        // GREEDY TARGET SELECTION
        // =====================================================
        // Hanya pilih musuh paling dekat
        // =====================================================
        double dist = DistanceTo(e.X, e.Y);

        if (!enemyDetected || dist < enemyDistance)
        {
            enemyDetected = true;

            enemyX = e.X;
            enemyY = e.Y;

            enemyDistance = dist;
        }

        // =====================================================
        // RADAR LOCK
        // =====================================================
        double radarTurn =
            NormalizeRelativeAngle(
                DirectionTo(e.X, e.Y)
                - RadarDirection);

        SetTurnRadarLeft(radarTurn);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // =====================================================
        // TABRAK + TEMBAK MAKSIMAL
        // =====================================================
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);

        // Tetap dorong musuh
        SetForward(150);

        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =====================================================
        // ANTI STUCK WALL
        // =====================================================
        SetBack(150);

        SetTurnLeft(90 + rnd.Next(90));

        SetForward(180);

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // =====================================================
        // SEDIKIT UBAH ARAH
        // =====================================================
        // Tetap agresif tapi tidak terlalu lurus
        SetTurnLeft(rnd.Next(-20, 20));

        SetForward(100);

        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // =====================================================
        // RESET TARGET
        // =====================================================
        enemyDetected = false;

        enemyDistance = double.MaxValue;

        SetTurnRadarRight(360);
    }
}