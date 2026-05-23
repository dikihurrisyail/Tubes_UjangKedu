using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/// <summary>
/// KicauMania2 - Bot Alternatif 1 (Greedy Strategy 2: Maximize Ram Damage)
/// 
/// STRATEGI GREEDY:
/// Heuristik: Selalu kejar dan tabrak musuh dengan energi paling rendah (hampir mati)
///            karena Ram Damage Bonus = 30% dari total damage ke musuh yang terbunuh dengan tabrak.
///            Secara greedy, memilih target lemah = kemungkinan Kill tertinggi = bonus terbesar.
/// 
/// Fungsi Objektif: Memaksimalkan Ram Damage + Ram Damage Bonus
/// 
/// Langkah Greedy:
///   1. Radar sweep terus mencari semua musuh.
///   2. Catat musuh dengan energi TERENDAH (target greedy = musuh paling lemah).
///   3. Maju langsung ke arah musuh lemah tersebut dengan kecepatan penuh (ramming).
///   4. Saat jarak dekat, tembak juga untuk memastikan musuh mati (kombinasi ram + bullet).
///   5. Jika sudah menabrak, langsung balik dan cari target lemah berikutnya.
/// </summary>
public class KicauMania2 : Bot
{
    private double _targetX    = 0;
    private double _targetY    = 0;
    private double _targetEnergy = double.MaxValue; // Musuh dengan energi terendah
    private bool   _hasTarget  = false;
    private int    _targetId   = -1;

    static void Main(string[] args)
    {
        new KicauMania2().Start();
    }

    KicauMania2() : base(BotInfo.FromFile("KicauMania2.json")) { }

    public override void Run()
    {
        BodyColor   = Color.FromArgb(0,   100, 0);   // hijau tua
        TurretColor = Color.FromArgb(50,  200, 50);  // hijau muda
        RadarColor  = Color.FromArgb(200, 255, 0);   // kuning-hijau
        BulletColor = Color.FromArgb(255, 255, 0);   // kuning

        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn   = true;
        AdjustRadarForGunTurn  = true;

        while (IsRunning)
        {
            // Radar sweep 360 derajat untuk mendeteksi semua musuh
            SetTurnRadarLeft(45);

            if (_hasTarget)
            {
                // GREEDY: Arahkan body ke musuh lemah dan maju menabrak
                double angleToTarget = DirectionTo(_targetX, _targetY);
                double bodyTurn = NormalizeRelativeAngle(angleToTarget - Direction);
                SetTurnLeft(bodyTurn);
                SetForward(double.MaxValue); // Maju penuh (ram)

                // Arahkan gun juga ke target untuk tembakan pendukung
                double gunTurn = NormalizeRelativeAngle(angleToTarget - GunDirection);
                SetTurnGunLeft(gunTurn);

                // Tembak dengan firepower rendah agar gun cepat dingin
                // (utama tetap ram, bullet hanya tambahan)
                double dist = DistanceTo(_targetX, _targetY);
                if (dist < 300 && GunHeat == 0 && Energy > 15)
                    SetFire(1); // Firepower rendah, hemat energi, fokus ram
            }
            else
            {
                // Tidak ada target: gerak berputar mencari musuh
                SetTurnLeft(10);
                SetForward(50);
            }

            Execute();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // GREEDY: Pilih musuh dengan energi paling rendah sebagai target
        // Energi rendah = hampir mati = potensi Ram Damage Bonus tertinggi
        if (e.Energy < _targetEnergy || e.ScannedBotId == _targetId)
        {
            _targetEnergy = e.Energy;
            _targetX      = e.X;
            _targetY      = e.Y;
            _targetId     = e.ScannedBotId;
            _hasTarget    = true;
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Saat menabrak musuh, langsung tembak dan mundur sedikit lalu tabrak lagi
        if (GunHeat == 0 && Energy > 10)
            SetFire(3); // Tembak keras saat kontak langsung

        // Reset target supaya cari musuh lemah berikutnya
        _targetEnergy = double.MaxValue;
        _hasTarget    = false;
        Execute();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetBack(50);
        TurnLeft(45);
        Execute();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // Target mati, reset dan cari target baru
        if (e.VictimId == _targetId)
        {
            _hasTarget    = false;
            _targetEnergy = double.MaxValue;
            _targetId     = -1;
        }
    }
}
