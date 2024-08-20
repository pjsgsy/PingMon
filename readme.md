PingMon

A simple console ping monitoring tool written in .NET 8.

Monitors a list of host names over ICMP until stopped, diplaying the responses, min, max, avg, and jitter for the last 100 samples.

Also shows the number of lost responses and a graphical bar indicating response times. This is coloured based on the average response time for that host so far.

Add your default hosts to monitor in the code, or supply a list of hostnames on startup.

For example

./pingmon.exe 192.168.2.1 192.168.2.43

![Alt text](WindowsTerminal_GKhN6YCEpd.jpg)

Written over a the course of a few cups of coffee today, so, may still contain bugs, etc!

