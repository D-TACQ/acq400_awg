# acq400_awg
program the acq400 AWG feature in C#
sample Session:
```
PS C:\Users\pgm00> c:\users\pgm00\source\repos\acq400_awg\acq400_awg\bin\Debug\acq400_awg.exe acq1001_301 10 aa bb
acq400_awg acq1001_301 10 aa bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb
acq1001_301 rep 10 load aa
acq1001_301 rep 10 load bb



USE bash script ./make-blobs to create large size files
On windows, use git-bash

## BETTER .. create 512MB data file: scott1M-512.dat
./bloat scott1M.dat 512
## create ramp1M-512.dat
./bload ramp1M.dat 512
