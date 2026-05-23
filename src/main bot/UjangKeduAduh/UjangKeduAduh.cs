using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// ============================================================
/// UjangKeduAduh
/// ============================================================
///
/// BOT TABRAK AGRESIF
///
/// PERBAIKAN:
/// - Anti nyangkut tembok
/// - Selalu kembali ke tengah arena
/// - Movement lebih stabil
/// - Tetap agresif menabrak musuh
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
            // ANTI WALL SYSTEM
            // =================================================
            // Jika dekat tembok:
            // langsung kembali ke tengah arena
            // =================================================
            double margin = 120;

            bool nearWall =
                X < margin ||
                X > ArenaWidth - margin ||
                Y < margin ||
                Y > ArenaHeight - margin;

            if (nearWall)
            {
                double centerAngle =
                    NormalizeRelativeAngle(
                        DirectionTo(
                            ArenaWidth / 2,
                            ArenaHeight / 2)
                        - Direction);

                SetTurnLeft(centerAngle);

                // Dorong jauh dari tembok
                SetForward(220);

                // Radar tetap jalan
                SetTurnRadarRight(360);

                Go();

                continue;
            }

            // =================================================
            // RADAR SWEEP
            // =================================================
            SetTurnRadarRight(45);

            // =================================================
            // MODE AGRESIF
            // =================================================
            if (enemyDetected)
            {
                // =============================================
                // ARAH KE MUSUH
                // =============================================
                double angleToEnemy =
                    DirectionTo(enemyX, enemyY);

                double bodyTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - Direction);

                // Putar body ke musuh
                SetTurnLeft(bodyTurn);

                // =============================================
                // MAJU TABRAK
                // =============================================
                SetForward(160);

                // =============================================
                // AIM GUN
                // =============================================
                double gunTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - GunDirection);

                SetTurnGunLeft(gunTurn);

                // =============================================
                // TEMBAK POWER MAKSIMAL
                // =============================================
                if (Math.Abs(gunTurn) < 15 &&
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

                SetForward(120);
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // =====================================================
        // UPDATE TARGET
        // =====================================================
        enemyX = e.X;
        enemyY = e.Y;

        enemyDetected = true;

        // =====================================================
        // RADAR LOCK
        // =====================================================
        double radarTurn =
            NormalizeRelativeAngle(
                DirectionTo(enemyX, enemyY)
                - RadarDirection);

        SetTurnRadarLeft(radarTurn);

        // =====================================================
        // TEMBAK INSTAN
        // =====================================================
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // =====================================================
        // CLOSE COMBAT
        // =====================================================
        // Saat tabrakan:
        // spam damage besar
        // =====================================================
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);

        // Tetap dorong musuh
        SetForward(120);

        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =====================================================
        // ANTI STUCK WALL
        // =====================================================
        // PERBAIKAN UTAMA:
        // - mundur jauh
        // - belok random besar
        // - kembali ke tengah
        // =====================================================

        // Mundur dari tembok
        SetBack(150);

        // Belok besar agar tidak nyangkut lagi
        SetTurnLeft(90 + rnd.Next(90));

        // Dorong ke arah baru
        SetForward(180);

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // =====================================================
        // TETAP AGRESIF
        // =====================================================
        // Saat ditembak:
        // sedikit ubah arah tapi tetap maju
        // =====================================================
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

        // Cari target baru
        SetTurnRadarRight(360);
    }
}