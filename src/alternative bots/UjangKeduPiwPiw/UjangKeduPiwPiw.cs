using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// =============================================================
/// UjangKeduPiwPiw
/// =============================================================
///
/// BOT ROBOCODE DENGAN STRATEGI GREEDY
///
/// =============================================================
/// STRATEGI UTAMA
/// =============================================================
///
/// Bot menggunakan pendekatan greedy:
/// yaitu selalu memilih aksi terbaik saat ini
/// berdasarkan kondisi lokal yang sedang terjadi.
///
/// Fokus utama:
/// 1. Memaksimalkan damage peluru
/// 2. Mempertahankan survival selama mungkin
///
/// =============================================================
/// FUNGSI OBJEKTIF
/// =============================================================
///
/// Memaksimalkan:
/// - Bullet Damage Score
/// - Bullet Kill Bonus
/// - Survival Score
///
/// =============================================================
/// LANGKAH GREEDY
/// =============================================================
///
/// 1. Radar terus sweep arena untuk mencari musuh
/// 2. Saat musuh ditemukan:
///    - hitung jarak musuh
///    - pilih firepower optimal
/// 3. Firepower dipilih greedy:
///    - dekat  -> power besar
///    - sedang -> power sedang
///    - jauh   -> power kecil
/// 4. Gun langsung diarahkan ke musuh
/// 5. Bot bergerak zigzag untuk menghindari peluru
/// 6. Jika energi rendah:
///    - prioritaskan survival
///    - hindari terlalu agresif
///
/// =============================================================
/// ALASAN STRATEGI GREEDY EFEKTIF
/// =============================================================
///
/// Karena Robocode adalah game real-time,
/// keputusan cepat tiap tick lebih penting
/// dibanding perencanaan jangka panjang.
///
/// Maka bot selalu memilih aksi terbaik lokal:
/// - tembakan paling efektif
/// - gerakan paling aman
/// - radar tercepat mendeteksi musuh
///
/// =============================================================
/// </summary>
public class UjangKeduPiwPiw : Bot
{
    // =========================================================
    // DATA MUSUH
    // =========================================================
    // Menyimpan posisi terakhir musuh
    // agar dapat digunakan untuk aiming dan targeting
    // =========================================================
    private double _enemyX = 0;
    private double _enemyY = 0;

    // True jika musuh sedang terdeteksi radar
    private bool _enemyDetected = false;

    // =========================================================
    // MOVEMENT CONTROL
    // =========================================================

    // Counter untuk mengatur pola zigzag
    private int _moveCounter = 0;

    // Arah gerakan:
    //  1  = maju
    // -1  = mundur
    private int _moveDirection = 1;

    static void Main(string[] args)
    {
        new UjangKeduPiwPiw().Start();
    }

    UjangKeduPiwPiw()
        : base(BotInfo.FromFile("UjangKeduPiwPiw.json")) { }

    public override void Run()
    {
        // =====================================================
        // PENGATURAN WARNA BOT
        // =====================================================
        // Desain merah-hitam
        // =====================================================
        BodyColor    = Color.Black;   // hitam
        TurretColor  = Color.Red;   // merah
        RadarColor   = Color.Yellow;    // kuning
        BulletColor  = Color.Orange;    // oranye
        ScanColor    = Color.Yellow;    // kuning

        // =====================================================
        // RADAR DAN GUN INDEPENDEN
        // =====================================================
        // GREEDY:
        // Agar radar, gun, dan body dapat bekerja
        // secara bersamaan tanpa saling mengganggu.
        //
        // Keuntungan:
        // - Radar tetap scan
        // - Gun tetap aiming
        // - Body tetap dodge
        // =====================================================
        AdjustRadarForBodyTurn   = true;
        AdjustGunForBodyTurn     = true;
        AdjustRadarForGunTurn    = true;

        while (IsRunning)
        {
            // =================================================
            // SURVIVAL MOVEMENT GREEDY
            // =================================================
            //
            // Objective:
            // Menghindari peluru musuh.
            //
            // Strategi greedy:
            // Bot terus bergerak zigzag karena:
            // - target diam mudah ditembak
            // - perubahan arah mengurangi akurasi musuh
            //
            // =================================================
            _moveCounter++;

            // Setiap 20 tick:
            // - balik arah
            // - ubah sudut gerakan
            if (_moveCounter % 20 == 0)
            {
                // Balik arah zigzag
                _moveDirection *= -1;

                // Belok random sedikit
                // agar movement tidak mudah diprediksi
                TurnLeft(30 + new Random().Next(30));
            }

            // =================================================
            // GERAK MAJU / MUNDUR
            // =================================================
            //
            // GREEDY:
            // Selalu bergerak untuk survival.
            //
            // =================================================
            if (_moveDirection == 1)
                SetForward(80);
            else
                SetBack(80);

            // =================================================
            // RADAR SWEEP GREEDY
            // =================================================
            //
            // Objective:
            // Menemukan musuh secepat mungkin.
            //
            // Strategi greedy:
            // Radar terus berputar agar coverage arena besar.
            //
            // =================================================
            SetTurnRadarLeft(45);

            // =================================================
            // ATTACK GREEDY
            // =================================================
            //
            // Objective:
            // Memaksimalkan damage dengan energi efisien.
            //
            // Bot hanya menyerang jika:
            // - musuh terdeteksi
            // - energi masih aman
            //
            // =================================================
            if (_enemyDetected && Energy > 20)
            {
                // Hitung jarak musuh
                double dist = DistanceTo(_enemyX, _enemyY);

                // =================================================
                // FIREPOWER GREEDY
                // =================================================
                //
                // Strategi:
                //
                // Dekat:
                //   gunakan power besar
                //   karena peluang hit tinggi
                //
                // Sedang:
                //   gunakan power sedang
                //
                // Jauh:
                //   gunakan power kecil
                //   karena peluru lebih cepat
                //
                // =================================================
                double firePower;

                if (dist < 200)
                    firePower = 3.0;

                else if (dist < 400)
                    firePower = 2.0;

                else
                    firePower = 1.0;

                // =================================================
                // TARGETING GREEDY
                // =================================================
                //
                // Objective:
                // Mengarahkan gun ke posisi musuh
                // secepat mungkin.
                //
                // =================================================
                double angleToEnemy =
                    DirectionTo(_enemyX, _enemyY);

                // Hitung selisih sudut gun dengan target
                double gunTurn =
                    NormalizeRelativeAngle(
                        angleToEnemy - GunDirection);

                // Putar gun menuju musuh
                SetTurnGunLeft(gunTurn);

                // =================================================
                // ACCURACY GREEDY
                // =================================================
                //
                // Objective:
                // Mengurangi peluru terbuang.
                //
                // Strategi greedy:
                // Hanya tembak jika aim cukup presisi.
                //
                // =================================================
                if (Math.Abs(gunTurn) < 10 &&
                    GunHeat == 0)
                {
                    SetFire(firePower);
                }
            }

            // =================================================
            // EKSEKUSI SEMUA COMMAND
            // =================================================
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // =====================================================
        // UPDATE DATA MUSUH
        // =====================================================
        //
        // Menyimpan posisi musuh terbaru
        // agar bisa digunakan oleh targeting system.
        //
        // =====================================================
        _enemyX = e.X;
        _enemyY = e.Y;

        _enemyDetected = true;

        // =====================================================
        // RADAR LOCK GREEDY
        // =====================================================
        //
        // Objective:
        // Menjaga radar tetap fokus pada musuh.
        //
        // Strategi greedy:
        // Begitu musuh terdeteksi,
        // radar langsung dikunci ke arah musuh.
        //
        // =====================================================
        double angleToEnemy =
            DirectionTo(e.X, e.Y);

        double radarTurn =
            NormalizeRelativeAngle(
                angleToEnemy - RadarDirection);

        SetTurnRadarLeft(radarTurn);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =====================================================
        // WALL AVOIDANCE GREEDY
        // =====================================================
        //
        // Objective:
        // Menghindari stuck di tembok.
        //
        // Strategi greedy:
        // - balik arah
        // - menjauh dari tembok
        //
        // =====================================================

        _moveDirection *= -1;

        SetBack(50);

        TurnLeft(45);

        Go();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // =====================================================
        // CLOSE RANGE GREEDY
        // =====================================================
        //
        // Objective:
        // Maksimalkan damage saat sangat dekat.
        //
        // Strategi greedy:
        // Gunakan firepower maksimum
        // karena peluang hit hampir pasti.
        //
        // =====================================================
        if (Energy > 10 && GunHeat == 0)
            SetFire(3);

        // =====================================================
        // ESCAPE GREEDY
        // =====================================================
        //
        // Setelah tabrakan:
        // - mundur
        // - ubah arah
        //
        // agar tidak jadi target empuk.
        //
        // =====================================================
        _moveDirection *= -1;

        SetBack(50);

        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // =====================================================
        // TARGET RESET GREEDY
        // =====================================================
        //
        // Objective:
        // Segera mencari target baru.
        //
        // =====================================================
        _enemyDetected = false;
    }
}