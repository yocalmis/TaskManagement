-- phpMyAdmin SQL Dump
-- version 4.5.1
-- http://www.phpmyadmin.net
--
-- Anamakine: localhost
-- Üretim Zamanı: 25 Ara 2017, 17:31:57
-- Sunucu sürümü: 5.5.45
-- PHP Sürümü: 5.5.38

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Veritabanı: `gorevdb`
--

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `dosyalar`
--

CREATE TABLE `dosyalar` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `isim` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `url` varchar(200) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `firma_musavir`
--

CREATE TABLE `firma_musavir` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `isim` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `proje_sayisi` int(11) NOT NULL,
  `surec_sayisi` int(11) NOT NULL,
  `gorev_sayisi` int(11) NOT NULL,
  `musteri_sayisi` int(11) NOT NULL,
  `kullanici_sayisi` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `firma_adi` varchar(300) DEFAULT NULL,
  `vergi_dairesi` varchar(300) DEFAULT NULL,
  `vergi_no` varchar(200) DEFAULT NULL,
  `adres` text,
  `fm_tur` int(11) NOT NULL,
  `konum_periyot` int(11) NOT NULL,
  `baslangic_tarihi` datetime NOT NULL,
  `bitis_tarihi` datetime NOT NULL,
  `firma_mail` varchar(500) NOT NULL,
  `mail_pass` varchar(500) NOT NULL,
  `mail_ssl` varchar(500) NOT NULL,
  `mail_port` varchar(500) NOT NULL,
  `mail_host` varchar(500) NOT NULL,
  `musteri_no` text NOT NULL,
  `sms_header` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Tablo döküm verisi `firma_musavir`
--

INSERT INTO `firma_musavir` (`id`, `flag`, `vid`, `sort`, `date`, `isim`, `aciklama`, `proje_sayisi`, `surec_sayisi`, `gorev_sayisi`, `musteri_sayisi`, `kullanici_sayisi`, `ekleyen`, `url`, `firma_adi`, `vergi_dairesi`, `vergi_no`, `adres`, `fm_tur`, `konum_periyot`, `baslangic_tarihi`, `bitis_tarihi`, `firma_mail`, `mail_pass`, `mail_ssl`, `mail_port`, `mail_host`, `musteri_no`, `sms_header`) VALUES
(1, 1, 1, 1, '2017-12-20 01:50:39', '', 'Varulf Açıklama', 30, 30, 30, 30, 30, 2, 'varulf-yazilim', 'Varulf Yazılım', 'Çankaya VD', '123', 'Varulf Adres', 1, 15, '2017-01-01 00:00:00', '2019-01-01 00:00:00', 'varulfmail@gmail.com', '123', 'true', '587', 'smtp.gmail.com', '330548', 'GOREV YONET'),
(2, 1, 2, 2, '2017-12-22 15:56:42', '', '', 5, 5, 5, 50, 10, 2, 'deneme-mali-musavirlik', 'Deneme Mali Müşavirlik', 'Erciyes', '2343456543', 'Adres Adres Adres Adres Adres Adres', 2, 1, '2017-12-22 00:00:00', '2018-12-22 00:00:00', 'yocalmis@gmail.com', '', 'true', '', '', '330548', '');

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorevler`
--

CREATE TABLE `gorevler` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `firma_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `yuzde` int(11) NOT NULL,
  `baslangic_tarihi` datetime NOT NULL,
  `bitis_tarihi` datetime NOT NULL,
  `isim` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `durum` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `onaylayan_yetkili` int(11) NOT NULL,
  `tamamlanma_tarihi` datetime NOT NULL,
  `gorev_multiply` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorev_baglanti`
--

CREATE TABLE `gorev_baglanti` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `bagli_gorev` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorev_dosya`
--

CREATE TABLE `gorev_dosya` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `dosya_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorev_loglari`
--

CREATE TABLE `gorev_loglari` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `islem` varchar(500) NOT NULL,
  `gorevin_eski_durumu` int(11) NOT NULL,
  `gorevin_yeni_durumu` int(11) NOT NULL,
  `aciklama` text NOT NULL,
  `url` varchar(200) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorev_musteri`
--

CREATE TABLE `gorev_musteri` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `musteri_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `kullanici_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `gorev_proje`
--

CREATE TABLE `gorev_proje` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `proje_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `kullanicilar`
--

CREATE TABLE `kullanicilar` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `ad` varchar(500) NOT NULL,
  `soyad` varchar(500) NOT NULL,
  `password` varchar(200) NOT NULL,
  `email` varchar(200) NOT NULL,
  `tel` varchar(200) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `firma_id` int(11) NOT NULL,
  `kullanici_turu` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `satis_musteri_id` varchar(200) DEFAULT NULL,
  `sgk_no` varchar(300) DEFAULT NULL,
  `adres` text,
  `tc_no` varchar(300) DEFAULT NULL,
  `vergi_dairesi` varchar(300) DEFAULT NULL,
  `vergi_no` varchar(300) DEFAULT NULL,
  `reset_guid` varchar(300) DEFAULT NULL,
  `reset_guidexpiredate` datetime DEFAULT NULL,
  `mail_permission` int(11) NOT NULL,
  `sms_permission` int(11) NOT NULL,
  `mail_port` varchar(100) NOT NULL,
  `mail_ssl` varchar(100) NOT NULL,
  `mail_host` varchar(100) NOT NULL,
  `mail_psw` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Tablo döküm verisi `kullanicilar`
--

INSERT INTO `kullanicilar` (`id`, `flag`, `vid`, `sort`, `date`, `ad`, `soyad`, `password`, `email`, `tel`, `ekleyen`, `firma_id`, `kullanici_turu`, `url`, `satis_musteri_id`, `sgk_no`, `adres`, `tc_no`, `vergi_dairesi`, `vergi_no`, `reset_guid`, `reset_guidexpiredate`, `mail_permission`, `sms_permission`, `mail_port`, `mail_ssl`, `mail_host`, `mail_psw`) VALUES
(2, 1, 1, 1, '2017-08-03 12:55:02', 'Varulf', 'Admin', 'hFDsoBZlUW2a61MXdkkCt4SVUCY3yWGSyBsWg9MtaRoJZc8Df+youe2e5vxquPJ/zo93xP2bSkQqAPwxe4I35mFkbWlu', 'varulfyazilim@gmail.com', '05064768590', 0, 1, 1, 'varulf1-admin', NULL, '3333', 'Kavaklıdere mah. Kızılırmak cad. No:7/7 Çankaya/Ankara', '122', 'Çankaya VD', '4324324', 'b977a1f3-5a8c-471e-89ce-db2d90144f7f', '2017-08-08 12:55:02', 1, 1, '587', 'true', 'smtp.gmail.com', 'Bilem1579.'),
(6, 1, 2, 2, '2017-12-22 16:00:37', 'Yusuf', ' Öcalmış', 'PXeFkSq39sRSVXsSaxLWUUWDt45I8tw9mgcY8GE3B/r3VylOko0q727qPChy+uibqcFuCy6w67ruaQ3AyHHeDjEyMzQ1Ng==', 'yocalmis@gmail.com', '5517058305', 2, 2, 20, 'yusuf-ocalmis', NULL, '', ' Öcalmış Öcalmış Öcalmış Öcalmış Öcalmış Öcalmış', '45658480910', 'Erciyes', '', '4c113be9-0657-4f74-bfa7-75ee9fcca66b', '2017-12-27 16:00:37', 1, 1, '', '', '', '');

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `kullanici_gorev`
--

CREATE TABLE `kullanici_gorev` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `kullanici_musteri`
--

CREATE TABLE `kullanici_musteri` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `musteri_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `kullanici_proje`
--

CREATE TABLE `kullanici_proje` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `proje_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `mailler`
--

CREATE TABLE `mailler` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gonderen_id` int(11) NOT NULL,
  `gonderen_mail` varchar(300) NOT NULL,
  `alan_mail` varchar(300) NOT NULL,
  `konu` varchar(500) NOT NULL,
  `icerik` text NOT NULL,
  `hedef_id` int(11) NOT NULL,
  `hedef_tur` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `mail_grup_id` int(11) NOT NULL,
  `firma_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `mesajlar`
--

CREATE TABLE `mesajlar` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gonderen_id` int(11) NOT NULL,
  `alan_id` int(11) NOT NULL,
  `mesaj` text NOT NULL,
  `firma_id` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `parent_url` varchar(200) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `musteriler`
--

CREATE TABLE `musteriler` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `ad` varchar(500) NOT NULL,
  `soyad` varchar(500) NOT NULL,
  `firma` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `url` varchar(200) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `firma_adi` varchar(300) DEFAULT NULL,
  `vergi_dairesi` varchar(300) DEFAULT NULL,
  `vergi_no` varchar(300) DEFAULT NULL,
  `adres` text,
  `firma_id` int(11) NOT NULL,
  `tel` varchar(45) NOT NULL,
  `email` varchar(500) NOT NULL,
  `gsm` varchar(45) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `proje_musteri`
--

CREATE TABLE `proje_musteri` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `proje_id` int(11) NOT NULL,
  `musteri_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `kullanici_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `proje_surec`
--

CREATE TABLE `proje_surec` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `firma_id` int(11) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `tur` int(11) NOT NULL,
  `baslangic_tarihi` datetime NOT NULL,
  `bitis_tarihi` datetime NOT NULL,
  `isim` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `durum` int(11) NOT NULL,
  `yuzde` int(11) NOT NULL,
  `periyot_turu` int(11) NOT NULL,
  `parent_vid` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `mevcut_donem` int(11) NOT NULL,
  `onaylayan_yetkili` int(11) NOT NULL,
  `tamamlanma_tarihi` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `saha_takip`
--

CREATE TABLE `saha_takip` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `latitude` decimal(10,8) NOT NULL,
  `longitude` decimal(10,8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `sistem_ayarlari`
--

CREATE TABLE `sistem_ayarlari` (
  `id` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `mail_address` varchar(500) NOT NULL,
  `mail_pswd` varchar(500) NOT NULL,
  `mail_port` varchar(500) NOT NULL,
  `mail_ssl` varchar(500) NOT NULL,
  `mail_host` varchar(500) NOT NULL,
  `sms_header` varchar(500) NOT NULL,
  `sms_password` varchar(500) NOT NULL,
  `sms_username` varchar(500) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `flag` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `sistem_bildirimleri`
--

CREATE TABLE `sistem_bildirimleri` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `bildirim_turu` int(11) NOT NULL,
  `ilgili_id` int(11) NOT NULL,
  `kullanici_id` int(11) NOT NULL,
  `okundu` int(11) NOT NULL,
  `okunma_tarihi` datetime NOT NULL,
  `ilgili_url` varchar(300) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `mesaj` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `smsler`
--

CREATE TABLE `smsler` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gonderen_id` int(11) NOT NULL,
  `hedef_numara` varchar(300) NOT NULL,
  `icerik` text NOT NULL,
  `hedef_id` int(11) NOT NULL,
  `hedef_tur` int(11) NOT NULL,
  `url` varchar(200) NOT NULL,
  `sms_grup_id` int(11) NOT NULL,
  `firma_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `yapilacaklar`
--

CREATE TABLE `yapilacaklar` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `gorev_id` int(11) NOT NULL,
  `isim` varchar(500) NOT NULL,
  `aciklama` text NOT NULL,
  `url` varchar(200) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `durum` int(11) NOT NULL,
  `gerceklestiren_id` int(11) NOT NULL,
  `firma_id` int(11) NOT NULL,
  `tamamlanma_tarihi` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tablo için tablo yapısı `yardim`
--

CREATE TABLE `yardim` (
  `id` int(11) NOT NULL,
  `flag` int(11) NOT NULL,
  `vid` int(11) NOT NULL,
  `sort` int(11) NOT NULL,
  `date` datetime NOT NULL,
  `baslik` varchar(500) NOT NULL,
  `icerik` text NOT NULL,
  `video` varchar(500) NOT NULL,
  `ekleyen` int(11) NOT NULL,
  `url` varchar(200) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dökümü yapılmış tablolar için indeksler
--

--
-- Tablo için indeksler `dosyalar`
--
ALTER TABLE `dosyalar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_dosyalar` (`url`),
  ADD KEY `fk_gorev_dosya_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `firma_musavir`
--
ALTER TABLE `firma_musavir`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_firma` (`url`),
  ADD KEY `fk_ekleyen_firma_musavir` (`ekleyen`);

--
-- Tablo için indeksler `gorevler`
--
ALTER TABLE `gorevler`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_gorevler` (`url`),
  ADD KEY `fk_ekleyen_gorevler` (`ekleyen`);

--
-- Tablo için indeksler `gorev_baglanti`
--
ALTER TABLE `gorev_baglanti`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_gorev_baglanti_gorev_id` (`gorev_id`),
  ADD KEY `fk_gorev_baglanti_bagli_gorev` (`bagli_gorev`),
  ADD KEY `fk_gorev_baglanti_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `gorev_dosya`
--
ALTER TABLE `gorev_dosya`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_gorev_dosya_gorev` (`gorev_id`),
  ADD KEY `fk_gorev_dosya_dosya` (`dosya_id`),
  ADD KEY `fk_gorev_dosya2_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `gorev_loglari`
--
ALTER TABLE `gorev_loglari`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_musteriler` (`url`),
  ADD KEY `fk_gorev_loglari_gorev` (`gorev_id`),
  ADD KEY `fk_gorev_loglari_kullanici` (`kullanici_id`);

--
-- Tablo için indeksler `gorev_musteri`
--
ALTER TABLE `gorev_musteri`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_gorev_musteri_musteri` (`musteri_id`),
  ADD KEY `fk_gorev_musteri_gorev` (`gorev_id`),
  ADD KEY `fk_gorev_musteri_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `gorev_proje`
--
ALTER TABLE `gorev_proje`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_gorev_proje_gorev` (`gorev_id`),
  ADD KEY `fk_gorev_proje_proje` (`proje_id`);

--
-- Tablo için indeksler `kullanicilar`
--
ALTER TABLE `kullanicilar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_kullanici` (`url`),
  ADD UNIQUE KEY `email_unique` (`email`);

--
-- Tablo için indeksler `kullanici_gorev`
--
ALTER TABLE `kullanici_gorev`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_kullanici_gorev_kullanici` (`kullanici_id`),
  ADD KEY `fk_kullanici_gorev_gorev` (`gorev_id`),
  ADD KEY `fk_kullanici_gorev_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `kullanici_musteri`
--
ALTER TABLE `kullanici_musteri`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_kullanici_musteri_musteri` (`musteri_id`),
  ADD KEY `fk_kullanici_musteri_kullanici` (`kullanici_id`),
  ADD KEY `fk_kullanici_proje_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `kullanici_proje`
--
ALTER TABLE `kullanici_proje`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_kullanici_proje_kullanici` (`kullanici_id`),
  ADD KEY `fk_kullanici_proje_proje` (`proje_id`),
  ADD KEY `fk_kullanici_musteri_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `mailler`
--
ALTER TABLE `mailler`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_mailler_kullanici` (`gonderen_id`);

--
-- Tablo için indeksler `mesajlar`
--
ALTER TABLE `mesajlar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_mesajlar` (`url`),
  ADD KEY `fk_gonderen` (`gonderen_id`),
  ADD KEY `fk_alan` (`alan_id`);

--
-- Tablo için indeksler `musteriler`
--
ALTER TABLE `musteriler`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_musteriler` (`url`),
  ADD KEY `fk_ekleyen_musteriler` (`ekleyen`),
  ADD KEY `fk_musteri_firma` (`firma_id`);

--
-- Tablo için indeksler `proje_musteri`
--
ALTER TABLE `proje_musteri`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_proj_musteri_musteri` (`musteri_id`),
  ADD KEY `fk_proj_musteri_proj` (`proje_id`);

--
-- Tablo için indeksler `proje_surec`
--
ALTER TABLE `proje_surec`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_proje` (`url`),
  ADD KEY `fk_ekleyen` (`ekleyen`),
  ADD KEY `fk_proje_surec_firma` (`firma_id`);

--
-- Tablo için indeksler `saha_takip`
--
ALTER TABLE `saha_takip`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_saha_takip_kullanici` (`kullanici_id`);

--
-- Tablo için indeksler `sistem_ayarlari`
--
ALTER TABLE `sistem_ayarlari`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_sistem_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `sistem_bildirimleri`
--
ALTER TABLE `sistem_bildirimleri`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_sistem_bildirimleri_ekleyen` (`ekleyen`);

--
-- Tablo için indeksler `smsler`
--
ALTER TABLE `smsler`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_smsler` (`url`),
  ADD KEY `fk_smsler_kullanici` (`gonderen_id`);

--
-- Tablo için indeksler `yapilacaklar`
--
ALTER TABLE `yapilacaklar`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_yapilacaklar` (`url`),
  ADD KEY `fk_ekleyen_yapilacaklar` (`ekleyen`),
  ADD KEY `fk_gorev_yapilacaklar` (`gorev_id`),
  ADD KEY `fk_gerceklestiren_yapilacaklar` (`gerceklestiren_id`),
  ADD KEY `fk_yapilacaklar_firma` (`firma_id`);

--
-- Tablo için indeksler `yardim`
--
ALTER TABLE `yardim`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `url_unique_yardim` (`url`),
  ADD KEY `fk_ekleyen_yardim` (`ekleyen`);

--
-- Dökümü yapılmış tablolar için AUTO_INCREMENT değeri
--

--
-- Tablo için AUTO_INCREMENT değeri `dosyalar`
--
ALTER TABLE `dosyalar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `firma_musavir`
--
ALTER TABLE `firma_musavir`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;
--
-- Tablo için AUTO_INCREMENT değeri `gorevler`
--
ALTER TABLE `gorevler`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `gorev_baglanti`
--
ALTER TABLE `gorev_baglanti`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `gorev_dosya`
--
ALTER TABLE `gorev_dosya`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `gorev_loglari`
--
ALTER TABLE `gorev_loglari`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `gorev_musteri`
--
ALTER TABLE `gorev_musteri`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `gorev_proje`
--
ALTER TABLE `gorev_proje`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `kullanicilar`
--
ALTER TABLE `kullanicilar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;
--
-- Tablo için AUTO_INCREMENT değeri `kullanici_gorev`
--
ALTER TABLE `kullanici_gorev`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `kullanici_musteri`
--
ALTER TABLE `kullanici_musteri`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `kullanici_proje`
--
ALTER TABLE `kullanici_proje`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `mailler`
--
ALTER TABLE `mailler`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `mesajlar`
--
ALTER TABLE `mesajlar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `musteriler`
--
ALTER TABLE `musteriler`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `proje_musteri`
--
ALTER TABLE `proje_musteri`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `proje_surec`
--
ALTER TABLE `proje_surec`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `saha_takip`
--
ALTER TABLE `saha_takip`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `sistem_ayarlari`
--
ALTER TABLE `sistem_ayarlari`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `sistem_bildirimleri`
--
ALTER TABLE `sistem_bildirimleri`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `smsler`
--
ALTER TABLE `smsler`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `yapilacaklar`
--
ALTER TABLE `yapilacaklar`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Tablo için AUTO_INCREMENT değeri `yardim`
--
ALTER TABLE `yardim`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Dökümü yapılmış tablolar için kısıtlamalar
--

--
-- Tablo kısıtlamaları `dosyalar`
--
ALTER TABLE `dosyalar`
  ADD CONSTRAINT `fk_gorev_dosya_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `firma_musavir`
--
ALTER TABLE `firma_musavir`
  ADD CONSTRAINT `fk_ekleyen_firma_musavir` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `gorevler`
--
ALTER TABLE `gorevler`
  ADD CONSTRAINT `fk_ekleyen_gorevler` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `gorev_baglanti`
--
ALTER TABLE `gorev_baglanti`
  ADD CONSTRAINT `fk_gorev_baglanti_bagli_gorev` FOREIGN KEY (`bagli_gorev`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_gorev_baglanti_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_gorev_baglanti_gorev_id` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`);

--
-- Tablo kısıtlamaları `gorev_dosya`
--
ALTER TABLE `gorev_dosya`
  ADD CONSTRAINT `fk_gorev_dosya2_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_gorev_dosya_dosya` FOREIGN KEY (`dosya_id`) REFERENCES `dosyalar` (`id`),
  ADD CONSTRAINT `fk_gorev_dosya_gorev` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`);

--
-- Tablo kısıtlamaları `gorev_loglari`
--
ALTER TABLE `gorev_loglari`
  ADD CONSTRAINT `fk_gorev_loglari_gorev` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_gorev_loglari_kullanici` FOREIGN KEY (`kullanici_id`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `gorev_musteri`
--
ALTER TABLE `gorev_musteri`
  ADD CONSTRAINT `fk_gorev_musteri_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_gorev_musteri_gorev` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_gorev_musteri_musteri` FOREIGN KEY (`musteri_id`) REFERENCES `musteriler` (`id`);

--
-- Tablo kısıtlamaları `gorev_proje`
--
ALTER TABLE `gorev_proje`
  ADD CONSTRAINT `fk_gorev_proje_gorev` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_gorev_proje_proje` FOREIGN KEY (`proje_id`) REFERENCES `proje_surec` (`id`);

--
-- Tablo kısıtlamaları `kullanici_gorev`
--
ALTER TABLE `kullanici_gorev`
  ADD CONSTRAINT `fk_kullanici_gorev_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_kullanici_gorev_gorev` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_kullanici_gorev_kullanici` FOREIGN KEY (`kullanici_id`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `kullanici_musteri`
--
ALTER TABLE `kullanici_musteri`
  ADD CONSTRAINT `fk_kullanici_musteri_kullanici` FOREIGN KEY (`kullanici_id`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_kullanici_musteri_musteri` FOREIGN KEY (`musteri_id`) REFERENCES `musteriler` (`id`),
  ADD CONSTRAINT `fk_kullanici_proje_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `kullanici_proje`
--
ALTER TABLE `kullanici_proje`
  ADD CONSTRAINT `fk_kullanici_musteri_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_kullanici_proje_kullanici` FOREIGN KEY (`kullanici_id`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_kullanici_proje_proje` FOREIGN KEY (`proje_id`) REFERENCES `proje_surec` (`id`);

--
-- Tablo kısıtlamaları `mesajlar`
--
ALTER TABLE `mesajlar`
  ADD CONSTRAINT `fk_alan` FOREIGN KEY (`alan_id`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_gonderen` FOREIGN KEY (`gonderen_id`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `musteriler`
--
ALTER TABLE `musteriler`
  ADD CONSTRAINT `fk_ekleyen_musteriler` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_musteri_firma` FOREIGN KEY (`firma_id`) REFERENCES `firma_musavir` (`id`);

--
-- Tablo kısıtlamaları `proje_musteri`
--
ALTER TABLE `proje_musteri`
  ADD CONSTRAINT `fk_proj_musteri_musteri` FOREIGN KEY (`musteri_id`) REFERENCES `musteriler` (`id`),
  ADD CONSTRAINT `fk_proj_musteri_proj` FOREIGN KEY (`proje_id`) REFERENCES `proje_surec` (`id`);

--
-- Tablo kısıtlamaları `proje_surec`
--
ALTER TABLE `proje_surec`
  ADD CONSTRAINT `fk_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_proje_surec_firma` FOREIGN KEY (`firma_id`) REFERENCES `firma_musavir` (`id`);

--
-- Tablo kısıtlamaları `saha_takip`
--
ALTER TABLE `saha_takip`
  ADD CONSTRAINT `fk_saha_takip_kullanici` FOREIGN KEY (`kullanici_id`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `sistem_ayarlari`
--
ALTER TABLE `sistem_ayarlari`
  ADD CONSTRAINT `fk_sistem_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `sistem_bildirimleri`
--
ALTER TABLE `sistem_bildirimleri`
  ADD CONSTRAINT `fk_sistem_bildirimleri_ekleyen` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `smsler`
--
ALTER TABLE `smsler`
  ADD CONSTRAINT `fk_smsler_kullanici` FOREIGN KEY (`gonderen_id`) REFERENCES `kullanicilar` (`id`);

--
-- Tablo kısıtlamaları `yapilacaklar`
--
ALTER TABLE `yapilacaklar`
  ADD CONSTRAINT `fk_ekleyen_yapilacaklar` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`),
  ADD CONSTRAINT `fk_gorev_yapilacaklar` FOREIGN KEY (`gorev_id`) REFERENCES `gorevler` (`id`),
  ADD CONSTRAINT `fk_yapilacaklar_firma` FOREIGN KEY (`firma_id`) REFERENCES `firma_musavir` (`id`);

--
-- Tablo kısıtlamaları `yardim`
--
ALTER TABLE `yardim`
  ADD CONSTRAINT `fk_ekleyen_yardim` FOREIGN KEY (`ekleyen`) REFERENCES `kullanicilar` (`id`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
