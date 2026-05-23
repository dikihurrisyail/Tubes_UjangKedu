using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// KicauMania3 - Bot Alternatif 2 (Greedy Strategy 3: Maximize Survival Score)
/// 
/// STRATEGI GREEDY:
/// Heuristik: Selalu prioritaskan bertahan hidup (Survival Score = 50 poin per bot mati).
///            Secara greedy, bot memilih aksi yang meminimalkan risiko kena tembak:
///            - Menjaga jarak aman dari semua musuh (>400px)
///            - Bergerak melingkar mengelilingi arena (circular movement)
///            - Menembak hanya ketika benar-benar aman (musuh tidak dalam rentang 200px)
/// 
/// Fungsi Objektif: Memaksimalkan Survival Score + Last Survival Bonus
/// 
/// Langkah Greedy:
///   1. Radar sweep terus.
///   2. Hitung arah ke musuh terdekat.
///   3. Greedy: jika musuh terlalu dekat (<300px), langsung kabur ke arah berlawanan.
///   4. Jika aman (>300px), lanjutkan gerakan melingkar (orbit arena).
///   5. Tembak hanya ketika GunHeat == 0 dan musuh terdeteksi.
///   6. Pertahankan energi di atas 30 agar tidak mudah dinonaktifkan.
/// </summary>
public class KicauMania3 : Bot
{
    private double _nearestEnemyX      = 0;
    private double _nearestEnemyY      = 0;
    private double _nearestEnemyDist   = double.MaxValue;
    private bool   _enemyDetected      = false;

    // Arah orbit: 1 = searah jarum jam, -1 = berlawanan
    private int _orbitDir = 1;

    static void Main(string[] args)
    {
        new KicauMania3().Start();
    }

    KicauMania3() : base(BotInfo.FromFile("KicauMania3.json")) { }

    public override void Run()
    {
        BodyColor   = Color.FromArgb(0,   60,  120);  // biru tua
        TurretColor = Color.FromArgb(0,   150, 255);  // biru muda
        RadarColor  = Color.FromArgb(0,   255, 200);  // cyan
        BulletColor = Color.FromArgb(100, 200, 255);  // biru muda

        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn   = true;
        AdjustRadarForGunTurn  = true;

        while (IsRunning)
        {
            // Radar sweep penuh terus-menerus
            SetTurnRadarLeft(45);

            if (_enemyDetected)
            {
                // GREEDY: Cek apakah musuh terlalu dekat
                if (_nearestEnemyDist < 300)
                {
                    // KABUR: Putar body 180 derajat dari arah musuh + mundur cepat
                    double escapeAngle = DirectionTo(_nearestEnemyX, _nearestEnemyY) + 180;
                    double bodyTurn = NormalizeRelativeAngle(escapeAngle - Direction);
                    SetTurnLeft(bodyTurn);
                    SetForward(150); // Kabur dari musuh

                    // Tetap tembak meski kabur (firepower rendah agar tidak buang energi)
                    if (GunHeat == 0 && Energy > 30)
                    {
                        double gunTurn = NormalizeRelativeAngle(
                            DirectionTo(_nearestEnemyX, _nearestEnemyY) - GunDirection);
                        SetTurnGunLeft(gunTurn);
                        SetFire(1);
                    }
                }
                else
                {
                    // AMAN: Lakukan gerakan orbit melingkar (mengelilingi musuh)
                    // Orbit = maju + belok ke samping musuh
                    double angleToEnemy = DirectionTo(_nearestEnemyX, _nearestEnemyY);
                    // Belok 90 derajat dari arah musuh = orbit
                    double orbitAngle = angleToEnemy + (90 * _orbitDir);
                    double bodyTurn = NormalizeRelativeAngle(orbitAngle - Direction);
                    SetTurnLeft(bodyTurn);
                    SetForward(100);

                    // Arahkan gun ke musuh dan tembak saat aman
                    double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
                    SetTurnGunLeft(gunTurn);
                    if (Math.Abs(gunTurn) < 15 && GunHeat == 0 && Energy > 30)
                        SetFire(2); // Firepower sedang, tidak terlalu boros energi
                }
            }
            else
            {
                // Tidak ada musuh terdeteksi: gerak orbit di pinggir arena
                SetTurnLeft(5 * _orbitDir);
                SetForward(100);
            }

            // Reset jarak musuh terdekat untuk scan berikutnya
            _nearestEnemyDist  = double.MaxValue;
            _enemyDetected     = false;

            Execute();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // GREEDY: Hanya update target jika ini adalah musuh paling dekat
        double dist = DistanceTo(e.X, e.Y);
        if (dist < _nearestEnemyDist)
        {
            _nearestEnemyDist = dist;
            _nearestEnemyX    = e.X;
            _nearestEnemyY    = e.Y;
            _enemyDetected    = true;
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Balik orbit direction saat kena dinding, lalu mundur
        _orbitDir *= -1;
        SetBack(80);
        TurnLeft(60);
        Execute();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Jika kena bot, balik arah orbit dan kabur
        _orbitDir *= -1;
        SetBack(100);
        if (GunHeat == 0 && Energy > 10)
            SetFire(3); // Kesempatan tembak dari dekat
        Execute();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Saat kena peluru, ubah orbit direction untuk mengacak gerakan
        _orbitDir *= -1;
    }
}
