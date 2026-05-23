# README - Robocode Tank Royale Bot Collection

## Deskripsi Project

Project ini berisi beberapa bot permainan **Robocode Tank Royale** yang dibuat menggunakan bahasa **C# (.NET)** dengan pendekatan algoritma **Greedy**.

Bot yang dibuat:

* `UjangKeduDorDor`
* `UjangKeduAwoo`
* `UjangKeduPiwPiw`
* `UjangKeduAduh`

Setiap bot memiliki strategi greedy yang berbeda untuk memaksimalkan skor permainan berdasarkan kondisi lokal saat pertandingan berlangsung.

---

# Penjelasan Strategi Greedy Tiap Bot

---

## 1. UjangKeduDorDor

### Strategi Utama

Bot menggunakan kombinasi:

* Greedy Movement
* Predictive Targeting
* Dynamic Firepower
* Bullet Dodge Detection
* Radar Lock
* Anti-Wall Movement

### Implementasi Greedy

Pada setiap tick permainan, bot memilih aksi terbaik berdasarkan kondisi saat itu:

* Jika musuh dekat → gunakan firepower besar
* Jika musuh jauh → gunakan firepower kecil agar peluru lebih cepat
* Jika energi musuh turun → diasumsikan musuh menembak → langsung dodge
* Jika dekat tembok → segera kembali ke tengah arena
* Jika aiming akurat → langsung tembak

### Objective Function

Memaksimalkan:

* Bullet Damage
* Survival Score
* Hit Accuracy
* Kill Bonus

---

## 2. UjangKeduAwoo

### Strategi Utama

Bot menggunakan:

* Advanced Greedy AI
* Predictive Targeting
* Orbit Movement
* Dynamic Distance Control
* Anti-Linear Movement
* Wall Smoothing

### Implementasi Greedy

Bot selalu memilih posisi terbaik secara lokal:

* Menjaga jarak ideal terhadap musuh
* Bergerak orbit agar sulit ditembak
* Mengubah arah secara random untuk menghindari prediksi lawan
* Menggunakan predictive aiming untuk menembak posisi masa depan musuh
* Mengunci radar pada target

### Objective Function

Memaksimalkan:

* Akurasi tembakan
* Damage per shot
* Survival time
* Movement unpredictability

---

## 3. UjangKeduPiwPiw

### Strategi Utama

Bot menggunakan:

* Simple Greedy Strategy
* Zig-Zag Movement
* Radar Sweep
* Dynamic Firepower

### Implementasi Greedy

Bot memilih aksi paling efektif setiap tick:

* Radar terus sweep mencari musuh
* Gerakan zig-zag untuk mengurangi kemungkinan terkena peluru
* Firepower dipilih berdasarkan jarak:

  * Dekat → power besar
  * Sedang → power sedang
  * Jauh → power kecil
* Menembak hanya jika aim cukup akurat

### Objective Function

Memaksimalkan:

* Bullet Damage
* Survival Score
* Fire Efficiency

---

## 4. UjangKeduAduh

### Strategi Utama

Bot menggunakan:

* Aggressive Greedy Chase
* Close Combat Strategy
* Anti-Wall Escape System
* Radar Lock

### Implementasi Greedy

Bot fokus mengejar dan menekan musuh:

* Langsung mengejar posisi musuh
* Menembak maksimum saat jarak dekat
* Mengunci radar pada musuh
* Jika menabrak tembok → segera escape menuju tengah arena
* Menghindari peluru menggunakan random movement

### Objective Function

Memaksimalkan:

* Aggressive Damage
* Close Combat Win Rate
* Survival from Wall Trap

---

# Requirement Program

## Software yang Dibutuhkan

### 1. .NET SDK

Disarankan menggunakan:

* .NET 6 SDK atau lebih baru

Cek instalasi:

```bash
dotnet --version
```

---

### 2. Robocode Tank Royale

Install Robocode Tank Royale:

[https://robocode-dev.github.io/tank-royale/](https://robocode-dev.github.io/tank-royale/)

---

### 3. Java Runtime (JDK)

Karena Robocode Tank Royale berjalan menggunakan Java.

Disarankan:

* JDK 17 atau lebih baru

Cek instalasi:

```bash
java -version
```

---

# Struktur Project

Contoh struktur folder:

```text
bot/
│
├── UjangKeduDorDor/
│   ├── UjangKeduDorDor.cs
│   ├── UjangKeduDorDor.json
│   ├── UjangKeduDorDor.csproj
│   ├── UjangKeduDorDor.cmd
│   └── UjangKeduDorDor.sh
│
├── UjangKeduAwoo/
├── UjangKeduPiwPiw/
├── UjangKeduAduh/
```

---

# Cara Build / Compile Program

Masuk ke folder bot:

```bash
cd UjangKeduDorDor
```

Compile project:

```bash
dotnet build
```

Membersihkan hasil build:

```bash
dotnet clean
```

Menjalankan bot:

```bash
dotnet run
```

---

# Cara Menjalankan di Robocode Tank Royale

1. Jalankan server Robocode Tank Royale
2. Jalankan GUI Robocode
3. Pastikan bot sudah berada pada folder bots
4. Jalankan bot menggunakan:

```bash
dotnet run
```

5. Pilih bot pada arena battle

---

# Kendala Saat Development

Beberapa kendala yang dihadapi selama development:

## 1. Bot Sering Menabrak Tembok

Solusi:

* Menambahkan anti-wall system
* Wall smoothing
* Escape mode

---

## 2. Aim Tidak Akurat

Solusi:

* Menggunakan predictive targeting
* Menghitung posisi masa depan musuh berdasarkan:

  * speed
  * direction
  * bullet speed

---

## 3. Movement Mudah Diprediksi

Solusi:

* Menambahkan random zig-zag
* Orbit movement
* Random direction switch

---

## 4. Radar Kehilangan Musuh

Solusi:

* Menggunakan radar lock
* Overshoot radar angle

---

## 5. Bot Terlalu Boros Energi

Solusi:

* Dynamic firepower
* Energy-aware shooting

---

# Algoritma Greedy yang Digunakan

Seluruh bot menggunakan konsep:

## Greedy Decision Making

Artinya:

Pada setiap tick permainan, bot langsung memilih aksi yang dianggap paling optimal berdasarkan kondisi saat itu tanpa menghitung seluruh kemungkinan jangka panjang.

Contoh keputusan greedy:

* Menembak dengan power terbesar ketika peluang hit tinggi
* Dodge segera ketika musuh menembak
* Bergerak menjauh saat terlalu dekat
* Mendekat ketika musuh terlalu jauh

Karena Robocode bersifat real-time, pendekatan greedy efektif untuk menghasilkan keputusan cepat dan responsif.

---

# Author

Project ini dibuat oleh:

* Cornelius Adhi Prasetya
* Diki Hurrisyail
* Agit Fadillah
