@echo off

git clone https://github.com/VelvetThePanda/SilkBot
cd SilkBot
docker build . -t silk:latest
docker-compose -f ./docker-compose-local.yml up -d
