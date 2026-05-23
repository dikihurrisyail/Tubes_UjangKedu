using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class UjangKeduAduh : Bot
{
    double enemyX;
    double enemyY;
    bool enemyDetected = false;
    
    // ← TAMBAHAN: flag anti-stuck
    bool isEscapingWall = false;
    int escapeTicksLeft = 0;

    Random rnd = new Random();

    static void Main(string[] args) => new UjangKeduAduh().Start();

    UjangKeduAduh() : base(BotInfo.FromFile("UjangKeduAduh.json")) { }

    public override void Run()
    {
        BodyColor = Color.DarkRed;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.OrangeRed;
        ScanColor = Color.White;
        TracksColor = Color.Black;
        GunColor = Color.Firebrick;

        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning)
        {
            SetTurnRadarRight(360);

            // =========================================
            // PRIORITAS 1: KELUAR DARI TEMBOK
            // =========================================
            if (isEscapingWall)
            {
                // Mundur + belok jauh dari tembok
                SetBack(120);
                // Saat mundur, arahkan ke tengah arena
                double angleToCenter = DirectionTo(ArenaWidth / 2.0, ArenaHeight / 2.0);
                double turnToCenter = NormalizeRelativeAngle(angleToCenter - Direction);
                SetTurnLeft(turnToCenter);

                escapeTicksLeft--;
                if (escapeTicksLeft <= 0)
                    isEscapingWall = false;

                Go();
                continue; // ← skip logika chase saat escaping
            }

            // =========================================
            // LOGIKA NORMAL
            // =========================================
            if (enemyDetected)
            {
                double angleToEnemy = DirectionTo(enemyX, enemyY);
                double bodyTurn = NormalizeRelativeAngle(angleToEnemy - Direction);
                SetTurnLeft(bodyTurn);
                SetForward(200);

                double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
                SetTurnGunLeft(gunTurn);

                if (Math.Abs(gunTurn) < 15 && GunHeat == 0)
                    SetFire(3);
            }
            else
            {
                SetTurnLeft(25);
                SetForward(100);
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyX = e.X;
        enemyY = e.Y;
        enemyDetected = true;

        double radarTurn = NormalizeRelativeAngle(
            DirectionTo(enemyX, enemyY) - RadarDirection);
        SetTurnRadarLeft(radarTurn);

        if (GunHeat == 0)
            SetFire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);
        SetForward(150);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =====================================================
        // AKTIFKAN ESCAPE MODE — jangan langsung Go()
        // Biarkan loop utama yang handle via flag
        // =====================================================
        isEscapingWall = true;
        escapeTicksLeft = 8; // ~8 tick mundur = cukup jauh dari tembok

        // Langsung set gerakan escape di tick ini
        SetBack(120);
        double angleToCenter = DirectionTo(ArenaWidth / 2.0, ArenaHeight / 2.0);
        double turnToCenter = NormalizeRelativeAngle(angleToCenter - Direction);
        SetTurnLeft(turnToCenter);

        // Jangan set enemyDetected = false,
        // supaya radar tetap ingat posisi musuh
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Tetap agresif, tapi jangan gerak saat escaping tembok
        if (!isEscapingWall)
        {
            SetForward(120);
            if (rnd.Next(100) < 30)
                SetTurnLeft(rnd.Next(-25, 25));
            Go();
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyDetected = false;
        SetTurnRadarRight(360);
    }
}