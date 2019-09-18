docker build -t tfa-bot https://github.com/consensusnetworks/CryptoBot.git
docker run --restart=always -d -e "BOTURL=https://docs.google.com/spreadsheets/d/1D6QsKlXhJXN3HKxSxrLxi9gAvB5QfjP01nt9o-VvubU/edit?usp=sharing" --name bot tfa-bot
pause
#7a6eeb5709e3376a96f3951e7e760e44b9b9d46f3a35822471c2045baadecc55