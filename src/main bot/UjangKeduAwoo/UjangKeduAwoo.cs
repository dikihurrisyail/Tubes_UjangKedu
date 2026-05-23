using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// =============================================================
/// UjangKeduAwoo
/// =============================================================
///
/// ADVANCED GREEDY + PREDICTIVE BOT
///
/// =============================================================
/// STRATEGI UTAMA
/// =============================================================
///
/// Bot ini menggunakan kombinasi:
///
/// 1. Greedy Decision Making
/// 2. Predictive Targeting
/// 3. Anti Linear Movement
/// 4. Dynamic Distance Control
/// 5. Wall Smoothing
/// 6. Radar Lock
///
/// =============================================================
/// STRATEGI GREEDY
/// =============================================================
///
/// Setiap tick bot memilih aksi terbaik:
///
/// - Posisi paling aman
/// - Damage paling besar
/// - Sudut dodge terbaik
/// - Jarak tempur optimal
///
/// =============================================================
/// </summary>
public class UjangKeduAwoo : Bot
{
    // =========================================================
    // ENEMY DATA
    // =========================================================
    double enemyX;
    double enemyY;
    double enemySpeed;
    double enemyDirection;
    double enemyDistance;

    bool enemyDetected = false;

    // =========================================================
    // MOVEMENT
    // =========================================================
    int moveDirection = 1;

    int randomMoveTimer = 0;

    Random rnd = new Random();

    // =========================================================
    // KONSTANTA
    // =========================================================
    const double PreferredDistance = 250;
    const double WallMargin = 80;

    static void Main(string[] args)
    {
        new UjangKeduAwoo().Start();
    }

    public UjangKeduAwoo()
        : base(BotInfo.FromFile("UjangKeduAwoo.json"))
    { }

    // =========================================================
    // MAIN LOOP
    // =========================================================
    public override void Run()
    {
        // =====================================================
        // WARNA BOT
        // =====================================================
        BodyColor = Color.Black;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.OrangeRed;
        ScanColor = Color.Cyan;
        TracksColor = Color.DarkRed;
        GunColor = Color.Red;

        // =====================================================
        // INDEPENDENT MOVEMENT
        // =====================================================
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        // =====================================================
        // LOOP UTAMA
        // =====================================================
        while (IsRunning)
        {
            // =================================================
            // RADAR SWEEP
            // =================================================
            if (!enemyDetected)
            {
                SetTurnRadarRight(360);
            }

            // =================================================
            // MOVEMENT SYSTEM
            // =================================================
            DoMovement();

            Go();
        }
    }

    // =========================================================
    // MOVEMENT AI
    // =========================================================
    void DoMovement()
    {
        randomMoveTimer++;

        // =====================================================
        // RANDOM DIRECTION CHANGE
        // =====================================================
        // Supaya movement tidak mudah diprediksi
        // =====================================================
        if (randomMoveTimer > 25)
        {
            randomMoveTimer = 0;

            if (rnd.NextDouble() > 0.5)
                moveDirection *= -1;
        }

        // =====================================================
        // JIKA MUSUH TERDETEKSI
        // =====================================================
        if (enemyDetected)
        {
            // =================================================
            // ORBIT MOVEMENT
            // =================================================
            // Bergerak melingkari musuh
            // agar sulit ditembak
            // =================================================
            double angleToEnemy =
                DirectionTo(enemyX, enemyY);

            double orbitAngle =
                NormalizeRelativeAngle(
                    angleToEnemy - Direction + 90);

            orbitAngle -= (15 * moveDirection);

            SetTurnLeft(orbitAngle);

            // =================================================
            // DISTANCE CONTROL
            // =================================================
            // Menjaga jarak ideal
            // =================================================
            if (enemyDistance < 180)
            {
                SetBack(120);
            }
            else if (enemyDistance > 350)
            {
                SetForward(120);
            }
            else
            {
                SetForward(80 * moveDirection);
            }
        }
        else
        {
            // =================================================
            // SEARCH MOVEMENT
            // =================================================
            SetForward(100);
            SetTurnLeft(20);
        }

        // =====================================================
        // WALL SMOOTHING
        // =====================================================
        AvoidWalls();
    }

    // =========================================================
    // WALL AVOIDANCE
    // =========================================================
    void AvoidWalls()
    {
        if (X < WallMargin ||
            X > ArenaWidth - WallMargin ||
            Y < WallMargin ||
            Y > ArenaHeight - WallMargin)
        {
            moveDirection *= -1;

            SetBack(150);

            SetTurnLeft(60);
        }
    }

    // =========================================================
    // SCANNED BOT
    // =========================================================
    public override void OnScannedBot(ScannedBotEvent e)
    {
        enemyDetected = true;

        enemyX = e.X;
        enemyY = e.Y;
        enemySpeed = e.Speed;
        enemyDirection = e.Direction;
        enemyDistance = DistanceTo(e.X, e.Y);

        // =====================================================
        // RADAR LOCK
        // =====================================================
        double angleToEnemy =
            DirectionTo(e.X, e.Y);

        double radarTurn =
            NormalizeRelativeAngle(
                angleToEnemy - RadarDirection);

        // Overshoot radar supaya tidak lepas
        radarTurn += radarTurn < 0 ? -20 : 20;

        SetTurnRadarLeft(radarTurn);

        // =====================================================
        // PREDICTIVE TARGETING
        // =====================================================
        //
        // Prediksi posisi musuh berdasarkan:
        // - arah gerak
        // - kecepatan
        //
        // =====================================================

        double firePower;

        // =====================================================
        // FIREPOWER GREEDY
        // =====================================================
        if (enemyDistance < 150)
            firePower = 3.0;
        else if (enemyDistance < 300)
            firePower = 2.0;
        else
            firePower = 1.2;

        // Hemat energi jika darah rendah
        if (Energy < 20)
            firePower = 1.0;

        // =====================================================
        // HITUNG KECEPATAN PELURU
        // =====================================================
        double bulletSpeed =
            20 - (3 * firePower);

        // =====================================================
        // PREDICT FUTURE POSITION
        // =====================================================
        double time =
            enemyDistance / bulletSpeed;

        double futureX =
            enemyX +
            Math.Sin(enemyDirection * Math.PI / 180)
            * enemySpeed * time;

        double futureY =
            enemyY +
            Math.Cos(enemyDirection * Math.PI / 180)
            * enemySpeed * time;

        // =====================================================
        // BATASI AGAR TIDAK KELUAR ARENA
        // =====================================================
        futureX = Math.Max(
            18,
            Math.Min(ArenaWidth - 18, futureX));

        futureY = Math.Max(
            18,
            Math.Min(ArenaHeight - 18, futureY));

        // =====================================================
        // AIM KE POSISI PREDIKSI
        // =====================================================
        double futureAngle =
            DirectionTo(futureX, futureY);

        double gunTurn =
            NormalizeRelativeAngle(
                futureAngle - GunDirection);

        SetTurnGunLeft(gunTurn);

        // =====================================================
        // TEMBAK HANYA JIKA AIM SUDAH AKURAT
        // =====================================================
        if (Math.Abs(gunTurn) < 6
            && GunHeat == 0)
        {
            SetFire(firePower);
        }
    }

    // =========================================================
    // KENA TEMBOK
    // =========================================================
    public override void OnHitWall(HitWallEvent e)
    {
        moveDirection *= -1;

        SetBack(100);

        SetTurnLeft(90);

        Go();
    }

    // =========================================================
    // KENA BOT
    // =========================================================
    public override void OnHitBot(HitBotEvent e)
    {
        // =====================================================
        // CLOSE RANGE ATTACK
        // =====================================================
        if (GunHeat == 0 && Energy > 10)
        {
            SetFire(3);
        }

        // =====================================================
        // ESCAPE
        // =====================================================
        moveDirection *= -1;

        SetBack(80);

        SetTurnLeft(45);

        Go();
    }

    // =========================================================
    // KENA PELURU
    // =========================================================
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // =====================================================
        // ANTI TARGETING DODGE
        // =====================================================
        //
        // Saat terkena peluru:
        // - ubah arah mendadak
        // - random turn
        //
        // =====================================================
        moveDirection *= -1;

        SetTurnLeft(30 + rnd.Next(60));

        SetForward(150 * moveDirection);

        Go();
    }

    // =========================================================
    // MUSUH MATI
    // =========================================================
    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyDetected = false;

        // Cari target baru
        SetTurnRadarRight(360);
    }
}