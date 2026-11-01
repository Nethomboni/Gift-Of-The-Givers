\# JMeter Load Testing



\## Test Plan Description

This project contains a JMeter load testing plan used to test the performance and reliability of the Gift of the Givers web application.



\## Pages Tested

The test plan sends HTTP requests to the following pages:

\- Home page

\- Volunteer page

\- Donation page



\## How the Test Was Run

The load test was executed using Apache JMeter in non-GUI mode.



The following command was used:



```bash

jmeter -n -t "tests/load/Load.jmx" -l "tests/load/results.jtl" -e -o "tests/load/report"

```



\## Purpose

The purpose of this test was to measure the application's response times, throughput and performance under multiple users accessing the website simultaneously.

