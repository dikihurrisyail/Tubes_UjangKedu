# README - Robocode Tank Royale Bot Collection

## Deskripsi Project

Project ini berisi beberapa bot untuk game **Robocode Tank Royale** yang dibuat menggunakan bahasa **C# (.NET)** dengan pendekatan algoritma **Greedy**.

Bot yang dibuat pada project ini antara lain:

* `UjangKeduDorDor`
* `UjangKeduAwoo`
* `UjangKeduPiwPiw`
* `UjangKeduAduh`

Masing-masing bot memiliki strategi yang berbeda dalam mengambil keputusan selama pertandingan berlangsung. Tujuan utama dari bot-bot ini adalah mendapatkan skor setinggi mungkin dengan memaksimalkan damage, akurasi tembakan, dan kemampuan bertahan hidup.

---

# Penjelasan Strategi Greedy Setiap Bot

---

## 1. UjangKeduDorDor

### Strategi yang Digunakan

Bot ini menggunakan beberapa kombinasi strategi seperti:

* Predictive Targeting
* Greedy Movement
* Dynamic Firepower
* Radar Lock
* Bullet Dodge Detection
* Anti-Wall System

### Cara Kerja Greedy

Bot akan mengambil keputusan terbaik berdasarkan kondisi saat itu juga.

Contohnya:

* Jika musuh berada dekat → bot memakai power peluru besar
* Jika musuh jauh → bot memakai power kecil supaya peluru lebih cepat
* Jika energi musuh turun → diasumsikan musuh baru menembak lalu bot langsung dodge
* Jika terlalu dekat dengan tembok → bot bergerak kembali ke tengah arena
* Bot hanya menembak ketika arah gun sudah cukup presisi

### Objective Function

Bot difokuskan untuk:

* Memaksimalkan damage peluru
* Meningkatkan akurasi hit
* Bertahan hidup lebih lama
* Mendapatkan kill bonus

---

## 2. UjangKeduAwoo

### Strategi yang Digunakan

Bot ini menggunakan strategi yang lebih advanced, yaitu:

* Predictive Targeting
* Orbit Movement
* Dynamic Distance Control
* Anti Linear Movement
* Wall Smoothing
* Radar Lock

### Cara Kerja Greedy

Bot akan selalu mencari posisi terbaik secara lokal pada setiap tick permainan.

Contohnya:

* Menjaga jarak ideal dengan musuh
* Bergerak memutari musuh agar sulit ditembak
* Mengubah arah secara random supaya movement tidak mudah ditebak
* Menggunakan prediksi posisi musuh saat melakukan aiming
* Mengunci radar ke target agar tidak kehilangan musuh

### Objective Function

Tujuan utama bot:

* Meningkatkan akurasi tembakan
* Memberikan damage besar
* Bertahan hidup selama mungkin
* Membuat movement sulit diprediksi lawan

---

## 3. UjangKeduPiwPiw

### Strategi yang Digunakan

Bot ini menggunakan strategi greedy sederhana seperti:

* Zig-Zag Movement
* Radar Sweep
* Dynamic Firepower

### Cara Kerja Greedy

Bot memilih aksi yang paling efektif berdasarkan situasi saat ini.

Contohnya:

* Radar terus berputar untuk mencari musuh
* Bot bergerak zig-zag agar lebih sulit terkena peluru
* Firepower disesuaikan berdasarkan jarak musuh:

  * Dekat → power besar
  * Sedang → power sedang
  * Jauh → power kecil
* Bot hanya menembak ketika aim cukup akurat

### Objective Function

Fokus utama bot:

* Memaksimalkan damage
* Menghemat energi
* Meningkatkan survival rate

---

## 4. UjangKeduAduh

### Strategi yang Digunakan

Bot ini menggunakan strategi agresif seperti:

* Aggressive Chase
* Close Combat
* Radar Lock
* Anti-Wall Escape System

### Cara Kerja Greedy

Bot fokus mengejar dan menyerang musuh secepat mungkin.

Contohnya:

* Langsung mendekati posisi musuh
* Menggunakan firepower maksimum saat jarak dekat
* Radar terus mengunci musuh
* Jika terkena tembok → bot segera keluar menuju area aman
* Jika terkena peluru → bot melakukan gerakan acak untuk dodge

### Objective Function

Bot difokuskan untuk:

* Memberikan damage sebesar mungkin
* Menang pada close combat
* Menghindari stuck di tembok

---

# Requirement Program

## Software yang Dibutuhkan

### 1. .NET SDK

Disarankan menggunakan:

* .NET 6 atau versi lebih baru

Cek instalasi:

```bash
dotnet --version
```

---

### 2. Robocode Tank Royale

Install Robocode Tank Royale melalui website resmi:

[Robocode Tank Royale Official Website](https://robocode-dev.github.io/tank-royale/?utm_source=chatgpt.com)

---

### 3. Java Runtime (JDK)

Karena Robocode Tank Royale berjalan menggunakan Java, maka perlu menginstall JDK.

Disarankan menggunakan:

* JDK 21 atau lebih baru

Cek instalasi:

```bash
java -version
```

---

# Struktur Folder Project

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

# Cara Build dan Menjalankan Program

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
3. Pastikan file bot sudah berada di folder bots
4. Jalankan bot menggunakan command:

```bash
dotnet run
```

5. Pilih bot pada arena battle

---

# Kendala Saat Development

Selama proses development terdapat beberapa kendala, di antaranya:

## 1. Bot Sering Menabrak Tembok

Solusi yang dilakukan:

* Menambahkan anti-wall movement
* Wall smoothing
* Escape mode

---

## 2. Aim Kurang Akurat

Solusi:

* Menggunakan predictive targeting
* Menghitung prediksi posisi musuh berdasarkan:

  * speed
  * direction
  * bullet speed

---

## 3. Movement Mudah Diprediksi

Solusi:

* Menambahkan movement random
* Zig-zag movement
* Orbit movement
* Random direction switch

---

## 4. Radar Sering Kehilangan Musuh

Solusi:

* Menggunakan radar lock
* Menambahkan overshoot radar angle

---

## 5. Energi Bot Cepat Habis

Solusi:

* Dynamic firepower
* Pengaturan power peluru berdasarkan kondisi energi

---

# Algoritma Greedy yang Digunakan

Semua bot pada project ini menggunakan konsep **Greedy Algorithm**.

Artinya, pada setiap tick permainan bot akan langsung memilih aksi yang dianggap paling optimal berdasarkan kondisi saat itu tanpa memikirkan seluruh kemungkinan jangka panjang.

Contoh keputusan greedy:

* Menggunakan power besar ketika peluang hit tinggi
* Langsung dodge ketika mendeteksi musuh menembak
* Menjauh saat posisi terlalu dekat
* Mendekat saat musuh terlalu jauh

Pendekatan greedy cocok digunakan pada Robocode karena permainan berjalan secara real-time sehingga bot harus mengambil keputusan dengan cepat.

---

# Author

Project ini dibuat oleh:

* Cornelius Adhi Prasetya
* Diki Hurrisyail
* Agit Fadillah