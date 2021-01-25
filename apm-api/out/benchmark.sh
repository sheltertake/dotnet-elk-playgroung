#!/bin/sh

/usr/local/bin/wrk -t12 -c50 -d5s --latency http://api/weatherforecast?count=3

sleep 5

wget http://api/throw

sleep 3

/usr/local/bin/wrk -t12 -c50 -d20s --latency http://api/weatherforecast?count=100000

sleep 10

wget http://api/throw

sleep 5

/usr/local/bin/wrk -t12 -c50 -d5s --latency http://api/weatherforecast?count=5
