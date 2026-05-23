using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class UjangKeduAduh : Bot
{
    // =========================================================
    // POSISI MUSUH TERAKHIR
    // =========================================================
    // Digunakan untuk:
    // - mengejar musuh
    // - aiming gun
    // =========================================================
    double enemyX;
    double enemyY;
    bool enemyDetected = false;

    // =========================================================
    // SISTEM ANTI-STUCK TEMBOK
    // =========================================================
    // Digunakan untuk:
    // - mendeteksi apakah bot sedang kabur dari tembok
    // - menghitung sisa tick mode escape
    // =========================================================
    bool isEscapingWall = false;
    int escapeTicksLeft = 0;

    // =========================================================
    // UTILITAS UMUM
    // =========================================================
    // Digunakan untuk:
    // - keputusan acak saat kena peluru
    // =========================================================
    Random rnd = new Random();

    static void Main(string[] args) => new UjangKeduAduh().Start();

    UjangKeduAduh() : base(BotInfo.FromFile("UjangKeduAduh.json")) { }

    public override void Run()
    {
        // =========================================================
        // KONFIGURASI TAMPILAN BOT
        // =========================================================
        // Digunakan untuk:
        // - memberi identitas visual pada bot di arena
        // =========================================================
        BodyColor = Color.DarkRed;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.OrangeRed;
        ScanColor = Color.White;
        TracksColor = Color.Black;
        GunColor = Color.Firebrick;

        // =========================================================
        // KONFIGURASI SINKRONISASI GERAK
        // =========================================================
        // Digunakan untuk:
        // - memisahkan rotasi radar dari badan & gun
        // - agar radar bisa berputar bebas saat bot bergerak
        // =========================================================
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning)
        {
            // =========================================================
            // RADAR SWEEP KONSTAN
            // =========================================================
            // Digunakan untuk:
            // - memastikan radar terus berputar mendeteksi musuh
            // =========================================================
            SetTurnRadarRight(360);

            // =========================================================
            // PRIORITAS 1: KELUAR DARI TEMBOK
            // =========================================================
            // Digunakan untuk:
            // - menangani kondisi bot menabrak tembok
            // - skip semua logika normal sampai escape selesai
            // =========================================================
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

            // =========================================================
            // PRIORITAS 2: KEJAR DAN SERANG MUSUH
            // =========================================================
            // Digunakan untuk:
            // - mengarahkan badan & gun ke posisi musuh
            // - maju mendekati musuh dan tembak jika gun siap
            // =========================================================
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
            // =========================================================
            // PRIORITAS 3: MODE PATROLI (MUSUH BELUM TERDETEKSI)
            // =========================================================
            // Digunakan untuk:
            // - bergerak acak sambil menunggu radar menemukan musuh
            // =========================================================
            else
            {
                SetTurnLeft(25);
                SetForward(100);
            }

            Go();
        }
    }

    // =========================================================
    // EVENT: BOT MUSUH TERDETEKSI RADAR
    // =========================================================
    // Digunakan untuk:
    // - menyimpan posisi musuh terbaru
    // - mengunci radar ke arah musuh
    // - langsung tembak jika gun siap
    // =========================================================
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

    // =========================================================
    // EVENT: BOT MENABRAK BOT MUSUH
    // =========================================================
    // Digunakan untuk:
    // - memanfaatkan kontak langsung untuk tembakan jarak dekat
    // - mendorong maju agar tidak terkunci di posisi
    // =========================================================
    public override void OnHitBot(HitBotEvent e)
    {
        if (GunHeat == 0 && Energy > 1)
            SetFire(3);
        SetForward(150);
        Go();
    }

    // =========================================================
    // EVENT: BOT MENABRAK TEMBOK
    // =========================================================
    // Digunakan untuk:
    // - mengaktifkan mode escape dari tembok
    // - mengatur arah kembali ke tengah arena
    // =========================================================
    public override void OnHitWall(HitWallEvent e)
    {
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

    // =========================================================
    // EVENT: BOT KENA PELURU MUSUH
    // =========================================================
    // Digunakan untuk:
    // - melakukan manuver menghindar saat tidak sedang escape
    // - sesekali belok acak agar sulit diprediksi
    // =========================================================
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if (!isEscapingWall)
        {
            SetForward(120);
            if (rnd.Next(100) < 30)
                SetTurnLeft(rnd.Next(-25, 25));
            Go();
        }
    }

    // =========================================================
    // EVENT: BOT MUSUH MATI
    // =========================================================
    // Digunakan untuk:
    // - mereset status deteksi musuh
    // - memulai kembali radar sweep penuh mencari target baru
    // =========================================================
    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyDetected = false;
        SetTurnRadarRight(360);
    }
}