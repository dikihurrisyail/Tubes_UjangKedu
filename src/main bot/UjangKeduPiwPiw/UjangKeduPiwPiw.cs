using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// UjangKeduPiwPiw - Bot Utama (Greedy Strategy 1: Maximize Bullet Damage + Survival)
/// 
/// STRATEGI GREEDY:
/// Heuristik: Selalu tembak musuh terdekat dengan firepower optimal agar energi kembali
///            lebih banyak (gain = 3x firepower), sambil bergerak menghindar untuk survival.
/// 
/// Fungsi Objektif: Memaksimalkan Bullet Damage + Survival Score
/// 
/// Langkah Greedy:
///   1. Radar selalu berputar untuk mendeteksi musuh.
///   2. Saat musuh terdeteksi, hitung jarak musuh.
///   3. Pilih firepower secara greedy:
///      - Jarak dekat  (<200px) : firepower = 3 (peluru berat, damage besar)
///      - Jarak sedang (<400px) : firepower = 2
///      - Jarak jauh   (>=400px): firepower = 1 (peluru ringan, lebih mudah kena)
///   4. Langsung arahkan gun ke musuh dan tembak.
///   5. Body bergerak zigzag (maju-mundur + belok) untuk menghindari peluru musuh (survival).
///   6. Jika energi rendah (<20), hindari menembak dan prioritaskan survival (gerak acak).
/// </summary>
public class UjangKeduPiwPiw : Bot
{
    // Posisi musuh terakhir yang terdeteksi
    private double _enemyX = 0;
    private double _enemyY = 0;
    private bool _enemyDetected = false;

    // Counter untuk pola gerakan zigzag
    private int _moveCounter = 0;
    private int _moveDirection = 1; // 1 = maju, -1 = mundur

    static void Main(string[] args)
    {
        new UjangKeduPiwPiw().Start();
    }

    UjangKeduPiwPiw() : base(BotInfo.FromFile("UjangKeduPiwPiw.json")) { }

    public override void Run()
    {
        // Warna khas UjangKeduPiwPiw: merah-hitam seperti burung jalak
        BodyColor    = Color.FromArgb(20,  20,  20);   // hitam
        TurretColor  = Color.FromArgb(200, 30,  30);   // merah
        RadarColor   = Color.FromArgb(255, 200, 0);    // kuning
        BulletColor  = Color.FromArgb(255, 80,  0);    // oranye
        ScanColor    = Color.FromArgb(255, 200, 0);    // kuning

        // Radar berputar terus secara independen (pisahkan dari body & gun)
        AdjustRadarForBodyTurn   = true;
        AdjustGunForBodyTurn     = true;
        AdjustRadarForGunTurn    = true;

        while (IsRunning)
        {
            // --- GREEDY: Survival Movement (zigzag) ---
            // Bergerak zig-zag untuk menghindari peluru musuh
            _moveCounter++;

            if (_moveCounter % 20 == 0)
            {
                // Setiap 20 turn, balik arah (zigzag)
                _moveDirection *= -1;
                // Belok sedikit untuk mengubah jalur (tidak mudah diprediksi)
                TurnLeft(30 + new Random().Next(30));
            }

            // Gerak maju/mundur sesuai arah saat ini
            if (_moveDirection == 1)
                SetForward(80);
            else
                SetBack(80);

            // --- GREEDY: Radar selalu sweep ---
            // Putar radar 45 derajat tiap turn agar selalu mendeteksi musuh
            SetTurnRadarLeft(45);

            // --- GREEDY: Tembak jika musuh terdeteksi dan energi cukup ---
            if (_enemyDetected && Energy > 20)
            {
                double dist = DistanceTo(_enemyX, _enemyY);

                // Pilih firepower secara greedy berdasarkan jarak
                // Gain energi = 3x firepower, jadi semakin besar firepower
                // semakin besar potensi gain, tapi peluru lebih lambat (perlu jarak dekat)
                double firePower;
                if (dist < 200)
                    firePower = 3.0; // Peluru berat: damage besar, gain energi besar
                else if (dist < 400)
                    firePower = 2.0; // Seimbang
                else
                    firePower = 1.0; // Peluru ringan: lebih mudah mengenai musuh jauh

                // Arahkan gun ke posisi musuh terdeteksi
                double angleToEnemy = DirectionTo(_enemyX, _enemyY);
                double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
                SetTurnGunLeft(gunTurn);

                // Tembak jika gun sudah cukup terarah (bearing < 10 derajat)
                if (Math.Abs(gunTurn) < 10 && GunHeat == 0)
                    SetFire(firePower);
            }

            Go(); // Kirim semua perintah ke server
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Simpan posisi musuh yang terdeteksi (untuk targeting)
        _enemyX = e.X;
        _enemyY = e.Y;
        _enemyDetected = true;

        // GREEDY: Langsung lock radar ke musuh yang baru di-scan
        // agar terus terpantau di turn berikutnya
        double angleToEnemy = DirectionTo(e.X, e.Y);
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        SetTurnRadarLeft(radarTurn);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Balik arah saat menabrak dinding untuk menghindari wall damage terus-menerus
        _moveDirection *= -1;
        SetBack(50);
        TurnLeft(45);
        Go();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Jika menabrak bot musuh:
        // GREEDY: Tembak langsung dengan firepower tinggi (musuh sangat dekat = firepower 3)
        // sekaligus mundur agar tidak kena balik
        if (Energy > 10 && GunHeat == 0)
            SetFire(3);

        _moveDirection *= -1; // Mundur dari tabrakan
        SetBack(50);
        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // Saat bot musuh mati, reset deteksi agar radar mencari musuh baru
        _enemyDetected = false;
    }
}
