@echo off

git pull
docker build . -t silk:latest
docker-compose -f ./docker-compose-local.yml up -d