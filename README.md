# 🧡 DA-Integration
![alt-текст](https://i.imgur.com/PZIsLTG.png "MMM баннер")

<p align="center">
  <img alt="GitHub Downloads (all assets, all releases)" src="https://img.shields.io/github/downloads/fet1sov/DA-Integration/total?label=%D0%A1%D0%BA%D0%B0%D1%87%D0%B8%D0%B2%D0%B0%D0%BD%D0%B8%D0%B9&color=%2303fc0b">
</p>


#### Простой плагин для сервера на ядре [Tshock](https://github.com/Pryaxis/TShock) который принимает события в реальном времени с сервиса приёма пожертвований [DonationAlerts](https://www.donationalerts.com/). Данный плагин взаимодействует с сервером [Centifugo](https://centrifugal.github.io/centrifugo/) от [DonationAlerts](https://www.donationalerts.com/) в реальном времени.

# 🌳 Пример работы 
![alt-текст](https://i.imgur.com/6yuSXeT.gif "Пример вывода доната в чат")


# 💾 Установка плагина
* Перейдите во вкладку `Releases`, скачайте **последнюю версию плагина**
* Распакуйте скачанные файлы из архива в папку `ServerPlugins` в директории сервера
* Перейдите в папку `ServerPlugins\DAIntegration` в директории сервера
* Затем откройте файл `config.ini`, и после знака равно в строке `authcode` вставьте ваш код аутентификации. 

**Для получение кода аутентификации:**
* [Перейдите по этой ссылке](https://donationalerts.com/oauth/authorize?client_id=9485&redirect_url=https://example.com/&response_type=code&scope=oauth-user-show%20oauth-donation-subscribe%20oauth-donation-index%20oauth-custom_alert-store)
* Нажмите на кнопку разрешить, вас переадресует на сайт `example.com`
* Вам требуется скопировать код после знака равно из адресной строки без ссылки
![alt-текст](https://i.imgur.com/kAZdV9m.png "Где находиться код")

**ВАЖНО:** Код аутентификации нужно обновлять после каждого перезапуска сервера

# ☕ Поддержка
[**Спасибки, если заглянешь ко мне на стрим :)**](https://www.youtube.com/channel/UCgWZ8m2ag5WpMT76HE7Kw5w)