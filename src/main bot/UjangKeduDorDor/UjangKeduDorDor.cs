using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class UjangKeduDorDor : Bot
{
    // =========================================================
    // DATA MUSUH
    // =========================================================
    // Menyimpan informasi terakhir musuh yang discan
    // agar bot bisa mengambil keputusan greedy terbaik
    // berdasarkan kondisi saat ini.
    // =========================================================
    double ex, ey, edist, edir, espd;

    // Energi musuh dipakai untuk mendeteksi kapan musuh menembak
    double enemyEnergy = 100, lastEnemyEnergy = 100;

    // =========================================================
    // STATE BOT
    // =========================================================

    // True jika musuh berhasil ditemukan radar
    bool enemyDetected = false;

    // True jika diprediksi musuh baru saja menembak
    bool bulletDetected = false;

    // =========================================================
    // MOVEMENT CONTROL
    // =========================================================

    // Arah gerakan zig-zag/orbit
    int moveDir = 1;

    // Counter waktu gerakan
    int moveTick = 0;

    // =========================================================
    // FIRE CONTROL
    // =========================================================

    // Cooldown agar bot tidak spam peluru
    int fireCooldown = 0;

    // Menyimpan firepower terakhir
    double lastFirePower = 1;

    // =========================================================
    // VIRTUAL ENERGY REWARD
    // =========================================================
    // Bonus virtual sebagai indikator greedy reward.
    // Semakin banyak hit → semakin agresif.
    // =========================================================
    double bonusEnergy = 0;

    Random rnd = new Random();

    static void Main(string[] args)
        => new UjangKeduDorDor().Start();

    UjangKeduDorDor()
        : base(BotInfo.FromFile("UjangKeduDorDor.json")) { }

    double Rad(double d) => d * Math.PI / 180;

    double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public override void Run()
    {
        // =========================================================
        // WARNA BOT
        // =========================================================
        BodyColor = Color.Pink;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        BulletColor = Color.Orange;
        ScanColor = Color.Cyan;
        TracksColor = Color.Gray;
        GunColor = Color.White;

        // =========================================================
        // RADAR DAN GUN INDEPENDEN
        // =========================================================
        // Greedy:
        // Agar radar, body, dan gun dapat bergerak sendiri-sendiri
        // sehingga keputusan lokal terbaik dapat dilakukan
        // secara bersamaan:
        // - Radar fokus scan
        // - Gun fokus aim
        // - Body fokus movement
        // =========================================================
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning)
        {
            moveTick++;

            // =====================================================
            // ANTI WALL GREEDY
            // =====================================================
            // Objective:
            // Menghindari tabrakan tembok karena:
            // - kehilangan momentum
            // - mudah ditembak
            // - posisi buruk
            //
            // Strategi greedy:
            // Jika dekat tembok → langsung pilih aksi lokal terbaik
            // yaitu kembali ke tengah arena.
            // =====================================================
            double margin = 140;

            double safeX = Clamp(X, margin, ArenaWidth - margin);
            double safeY = Clamp(Y, margin, ArenaHeight - margin);

            bool nearWall =
                X < margin ||
                X > ArenaWidth - margin ||
                Y < margin ||
                Y > ArenaHeight - margin;

            if (nearWall)
            {
                double centerAngle =
                    NormalizeRelativeAngle(
                        DirectionTo(ArenaWidth / 2, ArenaHeight / 2)
                        - Direction);

                SetTurnLeft(centerAngle);

                // Greedy:
                // Langsung bergerak ke area paling aman
                SetForward(160);

                Go();

                continue;
            }

            if (fireCooldown > 0)
                fireCooldown--;

            // =====================================================
            // RADAR SCANNING GREEDY
            // =====================================================
            // Objective:
            // Menemukan musuh secepat mungkin.
            //
            // Strategi greedy:
            // Radar terus berputar untuk memaksimalkan peluang
            // mendeteksi musuh setiap tick.
            // =====================================================
            SetTurnRadarRight(45);

            // =====================================================
            // MOVEMENT GREEDY
            // =====================================================
            if (enemyDetected)
            {
                // =================================================
                // STRAFING / ORBIT GREEDY
                // =================================================
                // Objective:
                // Tetap bergerak agar sulit ditembak
                // sambil mempertahankan posisi ideal.
                //
                // Strategi greedy:
                // Pilih sudut gerakan yang memberi:
                // - peluang dodge tinggi
                // - tetap bisa menyerang
                // =================================================
                double angle = NormalizeRelativeAngle(
                    DirectionTo(ex, ey) - Direction + 35 * moveDir);

                // =================================================
                // BULLET DODGE GREEDY
                // =================================================
                // Objective:
                // Menghindari peluru secepat mungkin.
                //
                // Strategi greedy:
                // Begitu energi musuh turun,
                // diasumsikan musuh menembak.
                //
                // Bot langsung:
                // - ganti arah
                // - bergerak random
                //
                // untuk memaksimalkan peluang dodge lokal.
                // =================================================
                if (bulletDetected)
                {
                    moveDir *= -1;

                    SetTurnLeft(70 * moveDir + rnd.Next(-15, 15));

                    SetForward(60);

                    bulletDetected = false;
                }
                else
                {
                    // =============================================
                    // DISTANCE CONTROL GREEDY
                    // =============================================
                    // Objective:
                    // Menentukan jarak terbaik terhadap musuh.
                    //
                    // Strategi greedy:
                    // - Jika jauh → mendekat agar aim lebih mudah
                    // - Jika sedang → maintain pressure
                    // - Jika dekat → orbit agar sulit ditembak
                    // =============================================
                    SetTurnLeft(angle);

                    if (edist > 400)
                        SetForward(80);

                    else if (edist > 180)
                        SetForward(90);

                    else
                    {
                        // Orbit ketika terlalu dekat
                        SetTurnLeft(45 * moveDir);
                        SetForward(60);
                    }

                    // =================================================
                    // RANDOMIZED MOVEMENT GREEDY
                    // =================================================
                    // Objective:
                    // Mengurangi prediksi musuh.
                    //
                    // Strategi greedy:
                    // Mengubah arah berkala agar lawan
                    // sulit memprediksi movement.
                    // =================================================
                    if (moveTick % 30 == 0)
                        moveDir *= -1;
                }
            }
            else
            {
                // =================================================
                // SEARCH GREEDY
                // =================================================
                // Objective:
                // Menemukan musuh secepat mungkin.
                //
                // Strategi greedy:
                // Bergerak sambil scan untuk memaksimalkan coverage.
                // =================================================
                SetTurnLeft(15);
                SetForward(80);
            }

            // =====================================================
            // TARGETING GREEDY
            // =====================================================
            if (enemyDetected)
            {
                double totalEnergy = Energy + bonusEnergy;

                // =================================================
                // FIREPOWER GREEDY
                // =================================================
                // Objective:
                // Memaksimalkan damage namun tetap hemat energi.
                //
                // Strategi greedy:
                // - Dekat  -> power besar
                // - Sedang -> power sedang
                // - Jauh   -> power kecil
                //
                // karena peluang hit berbeda.
                // =================================================
                double fire =
                edist < 120 ? 3 :
                edist < 300 ? 2 :
                1;

                // =================================================
                // ENERGY MANAGEMENT GREEDY
                // =================================================
                // Objective:
                // Bertahan hidup lebih lama.
                //
                // Strategi greedy:
                // Jika energi rendah,
                // kurangi firepower untuk survival.
                // =================================================
                if (totalEnergy < 25 && edist > 120)
                    fire = 1;

                double realFire = Math.Min(fire, 3);

                // =================================================
                // PREDICTIVE AIM GREEDY
                // =================================================
                // Objective:
                // Menembak posisi masa depan musuh.
                //
                // Strategi greedy:
                // Menggunakan:
                // - arah musuh
                // - kecepatan musuh
                // - kecepatan peluru
                //
                // untuk memilih titik tembak terbaik lokal.
                // =================================================
                double bulletSpeed = 20 - (3 * realFire);

                double t = edist / bulletSpeed;

                double px =
                    ex + Math.Cos(Rad(edir)) * espd * t;

                double py =
                    ey + Math.Sin(Rad(edir)) * espd * t;

                // =================================================
                // ARENA CLAMP GREEDY
                // =================================================
                // Objective:
                // Menghindari aim keluar arena.
                // =================================================
                px = Math.Max(18, Math.Min(ArenaWidth - 18, px));
                py = Math.Max(18, Math.Min(ArenaHeight - 18, py));

                double gunTurn = NormalizeRelativeAngle(
                    DirectionTo(px, py) - GunDirection);

                SetTurnGunLeft(gunTurn);

                // =================================================
                // ACCURACY GREEDY
                // =================================================
                // Objective:
                // Mengurangi peluru sia-sia.
                //
                // Strategi greedy:
                // Hanya menembak jika aim hampir presisi.
                // =================================================
                if (Math.Abs(gunTurn) < 5 &&
                    GunHeat == 0 &&
                    fireCooldown == 0)
                {
                    lastFirePower = realFire;

                    SetFire(realFire);

                    // =================================================
                    // FIRE COOLDOWN GREEDY
                    // =================================================
                    // Objective:
                    // Menghindari spam dan menjaga akurasi.
                    // =================================================
                    fireCooldown =
                        realFire >= 3 ? 22 :
                        realFire >= 2 ? 16 : 10;
                }
            }

            // Eksekusi semua command
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // =========================================================
        // UPDATE DATA MUSUH
        // =========================================================
        enemyDetected = true;

        ex = e.X;
        ey = e.Y;

        edir = e.Direction;
        espd = e.Speed;

        edist = DistanceTo(ex, ey);

        // =========================================================
        // DETEKSI TEMBAKAN MUSUH GREEDY
        // =========================================================
        // Objective:
        // Dodge sebelum peluru sampai.
        //
        // Strategi greedy:
        // Jika energi musuh turun sedikit,
        // diasumsikan musuh baru menembak.
        // =========================================================
        lastEnemyEnergy = enemyEnergy;
        enemyEnergy = e.Energy;

        double drop = lastEnemyEnergy - enemyEnergy;

        if (drop > 0.1 && drop <= 3)
            bulletDetected = true;

        // =========================================================
        // RADAR LOCK GREEDY
        // =========================================================
        // Objective:
        // Menjaga radar tetap mengunci target.
        //
        // Strategi greedy:
        // Radar selalu diarahkan kembali ke musuh.
        // =========================================================
        double radarTurn = NormalizeRelativeAngle(
            DirectionTo(ex, ey) - RadarDirection);

        SetTurnRadarLeft(radarTurn);
    }

    public override void OnBulletHit(BulletHitBotEvent e)
    {
        // =========================================================
        // REWARD GREEDY
        // =========================================================
        // Objective:
        // Memberi reward ketika hit berhasil.
        //
        // Strategi greedy:
        // Semakin banyak hit → bot lebih percaya diri/agresif.
        // =========================================================
        bonusEnergy += lastFirePower * 3;

        // Sedikit pressure maju
        SetForward(40);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // =========================================================
        // EMERGENCY DODGE GREEDY
        // =========================================================
        // Objective:
        // Keluar dari jalur peluru berikutnya.
        //
        // Strategi greedy:
        // Langsung:
        // - ubah arah
        // - gerak random
        // =========================================================
        moveDir *= -1;

        SetTurnLeft(rnd.Next(50, 100));

        SetForward(70);

        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // =========================================================
        // WALL RECOVERY GREEDY
        // =========================================================
        // Objective:
        // Cepat kembali ke posisi aman.
        //
        // Strategi greedy:
        // Langsung arahkan ke tengah arena.
        // =========================================================
        double angleToCenter =
            NormalizeRelativeAngle(
                DirectionTo(ArenaWidth / 2, ArenaHeight / 2)
                - Direction);

        SetTurnLeft(angleToCenter);

        SetForward(200);

        moveDir *= -1;

        Go();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // =========================================================
        // CLOSE COMBAT GREEDY
        // =========================================================
        // Objective:
        // Memaksimalkan damage saat jarak dekat.
        //
        // Strategi greedy:
        // Gunakan firepower maksimum saat tabrakan.
        // =========================================================
        if (GunHeat == 0 && Energy > 15)
            SetFire(3);

        // Menjauh setelah collision
        moveDir *= -1;

        SetBack(90);
        SetTurnLeft(45);

        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // =========================================================
        // TARGET SWITCH GREEDY
        // =========================================================
        // Objective:
        // Cepat menemukan target baru.
        // =========================================================
        enemyDetected = false;

        SetTurnRadarRight(360);
    }
}