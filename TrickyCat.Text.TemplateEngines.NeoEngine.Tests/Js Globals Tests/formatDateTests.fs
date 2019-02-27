﻿namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.JsGlobalsTests

open FsUnitTyped
open NUnit.Framework
open TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests.``Template Engine Tests Common``
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.TemplateRunnerHelpers

module ``'formatDate' Function Tests`` =
    let private formatDateFunctionSpec: obj [][] = [|
        [| "<%= formatDate(d, '%A') %>"; "Wed" |]
        [| "<%= formatDate(d, '%A') %>"; "Wed" |]
        [| "<%= formatDate(d, '%Al') %>"; "Wednesday" |]
        [| "<%= formatDate(d, '%B') %>"; "Jan" |]
        [| "<%= formatDate(d, '%B') %>"; "Jan" |]
        [| "<%= formatDate(d, '%Bl') %>"; "January" |]
        [| "<%= formatDate(d, '%D') %>"; "23" |]
        [| "<%= formatDate(d, '%1D') %>"; "3" |]
        [| "<%= formatDate(d, '%2D') %>"; "23" |]
        [| "<%= formatDate(d, '%3D') %>"; "023" |]
        [| "<%= formatDate(d, '%4D') %>"; "0023" |]
        [| "<%= formatDate(d, '%H') %>"; "11" |]
        [| "<%= formatDate(d, '%0H') %>"; "11" |]
        [| "<%= formatDate(d, '%1H') %>"; "1" |]
        [| "<%= formatDate(d, '%01H') %>"; "1" |]
        [| "<%= formatDate(d, '%2H') %>"; "11" |]
        [| "<%= formatDate(d, '%02H') %>"; "11" |]
        [| "<%= formatDate(d, '%3H') %>"; "011" |]
        [| "<%= formatDate(d, '%03H') %>"; "011" |]
        [| "<%= formatDate(d, '%4H') %>"; "0011" |]
        [| "<%= formatDate(d, '%04H') %>"; "0011" |]
        [| "<%= formatDate(d, '%I') %>"; "11" |]
        [| "<%= formatDate(d, '%0I') %>"; "11" |]
        [| "<%= formatDate(d, '%1I') %>"; "1" |]
        [| "<%= formatDate(d, '%01I') %>"; "1" |]
        [| "<%= formatDate(d, '%2I') %>"; "11" |]
        [| "<%= formatDate(d, '%02I') %>"; "11" |]
        [| "<%= formatDate(d, '%3I') %>"; "011" |]
        [| "<%= formatDate(d, '%03I') %>"; "011" |]
        [| "<%= formatDate(d, '%4I') %>"; "0011" |]
        [| "<%= formatDate(d, '%04I') %>"; "0011" |]
        [| "<%= formatDate(d, '%J') %>"; "23" |]
        [| "<%= formatDate(d, '%1J') %>"; "3" |]
        [| "<%= formatDate(d, '%2J') %>"; "23" |]
        [| "<%= formatDate(d, '%3J') %>"; "023" |]
        [| "<%= formatDate(d, '%4J') %>"; "0023" |]
        [| "<%= formatDate(d, '%M') %>"; "1" |]
        [| "<%= formatDate(d, '%1M') %>"; "1" |]
        [| "<%= formatDate(d, '%2M') %>"; "01" |]
        [| "<%= formatDate(d, '%3M') %>"; "001" |]
        [| "<%= formatDate(d, '%4M') %>"; "0001" |]
        [| "<%= formatDate(d, '%N') %>"; "41" |]
        [| "<%= formatDate(d, '%0N') %>"; "41" |]
        [| "<%= formatDate(d, '%1N') %>"; "1" |]
        [| "<%= formatDate(d, '%01N') %>"; "1" |]
        [| "<%= formatDate(d, '%2N') %>"; "41" |]
        [| "<%= formatDate(d, '%02N') %>"; "41" |]
        [| "<%= formatDate(d, '%3N') %>"; "041" |]
        [| "<%= formatDate(d, '%03N') %>"; "041" |]
        [| "<%= formatDate(d, '%4N') %>"; "0041" |]
        [| "<%= formatDate(d, '%04N') %>"; "0041" |]
        [| "<%= formatDate(d, '%P') %>"; "AM" |]
        [| "<%= formatDate(d, '%S') %>"; "25" |]
        [| "<%= formatDate(d, '%0S') %>"; "25" |]
        [| "<%= formatDate(d, '%1S') %>"; "5" |]
        [| "<%= formatDate(d, '%01S') %>"; "5" |]
        [| "<%= formatDate(d, '%2S') %>"; "25" |]
        [| "<%= formatDate(d, '%02S') %>"; "25" |]
        [| "<%= formatDate(d, '%3S') %>"; "025" |]
        [| "<%= formatDate(d, '%03S') %>"; "025" |]
        [| "<%= formatDate(d, '%4S') %>"; "0025" |]
        [| "<%= formatDate(d, '%04S') %>"; "0025" |]
        [| "<%= formatDate(d, '%W') %>"; "4" |]
        [| "<%= formatDate(d, '%1W') %>"; "4" |]
        [| "<%= formatDate(d, '%2W') %>"; "04" |]
        [| "<%= formatDate(d, '%3W') %>"; "004" |]
        [| "<%= formatDate(d, '%4W') %>"; "0004" |]
        [| "<%= formatDate(d, '%Y') %>"; "2019" |]
        [| "<%= formatDate(d, '%1Y') %>"; "9" |]
        [| "<%= formatDate(d, '%2Y') %>"; "19" |]
        [| "<%= formatDate(d, '%3Y') %>"; "019" |]
        [| "<%= formatDate(d, '%4Y') %>"; "2019" |]
        [| "<%= formatDate(d, '%5Y') %>"; "02019" |]
        [| "<%= formatDate(d, '%6Y') %>"; "002019" |]
        [| "<%= formatDate(d, '%7Y') %>"; "0002019" |]
        [| "<%= formatDate(d, '%2D %4W %1I %03N %4H') %>"; "23 0004 1 041 0011" |]
        [| "<%= formatDate(d, '%M %Bl %1W %1J %5Y') %>"; "1 January 4 3 02019" |]
        [| "<%= formatDate(d, '%N %P %03N %02N %Bl') %>"; "41 AM 041 41 January" |]
        [| "<%= formatDate(d, '%B %A %2H %01S %02N') %>"; "Jan Wed 11 5 41" |]
        [| "<%= formatDate(d, '%B %H %2Y %3Y %Y') %>"; "Jan 11 19 019 2019" |]
        [| "<%= formatDate(d, '%B %2D %3N %2N %3I') %>"; "Jan 23 041 41 011" |]
        [| "<%= formatDate(d, '%I %1M %4J %4D %B') %>"; "11 1 0023 0023 Jan" |]
        [| "<%= formatDate(d, '%0N %3M %04N %2M %I') %>"; "41 001 0041 01 11" |]
        [| "<%= formatDate(d, '%3W %6Y %4M %I %M') %>"; "004 002019 0001 11 1" |]
        [| "<%= formatDate(d, '%1I %J %3H %2I %02S') %>"; "1 23 011 11 25" |]
        [| "<%= formatDate(d, '%W %Al %4S %1Y %04S') %>"; "4 Wednesday 0025 9 0025" |]
        [| "<%= formatDate(d, '%4Y %5Y %3I %2S %2H') %>"; "2019 02019 011 25 11" |]
        [| "<%= formatDate(d, '%03I %1J %03H %Bl %P') %>"; "011 3 011 January AM" |]
        [| "<%= formatDate(d, '%B %2H %1M %0N %1M') %>"; "Jan 11 1 41 1" |]
        [| "<%= formatDate(d, '%4M %02I %S %3J %2S') %>"; "0001 11 25 023 25" |]
        [| "<%= formatDate(d, '%B %02N %3N %3J %1N') %>"; "Jan 41 041 023 1" |]
        [| "<%= formatDate(d, '%03N %3W %I %W %3M') %>"; "041 004 11 4 001" |]
        [| "<%= formatDate(d, '%4H %P %0N %1I %1D') %>"; "0011 AM 41 1 3" |]
        [| "<%= formatDate(d, '%01N %H %1Y %1I %Y') %>"; "1 11 9 1 2019" |]
        [| "<%= formatDate(d, '%1J %01S %4J %01N %2J') %>"; "3 5 0023 1 23" |]
        [| "<%= formatDate(d, '%2D %1Y %04N %W %3H') %>"; "23 9 0041 4 011" |]
        [| "<%= formatDate(d, '%M %01I %02I %0H %02S') %>"; "1 1 11 11 25" |]
        [| "<%= formatDate(d, '%2I %01I %1Y %1H %0N') %>"; "11 1 9 1 41" |]
        [| "<%= formatDate(d, '%1M %4D %04I %4M %1J') %>"; "1 0023 0011 0001 3" |]
        [| "<%= formatDate(d, '%02I %03N %01H %3J %Y') %>"; "11 041 1 023 2019" |]
        [| "<%= formatDate(d, '%03I %1J %4I %N %1I') %>"; "011 3 0011 41 1" |]
        [| "<%= formatDate(d, '%01I %H %B %A %4J') %>"; "1 11 Jan Wed 0023" |]
        [| "<%= formatDate(d, '%1S %01H %3J %2N %B') %>"; "5 1 023 41 Jan" |]
        [| "<%= formatDate(d, '%2Y %2I %4I %N %02S') %>"; "19 11 0011 41 25" |]
        [| "<%= formatDate(d, '%04H %03H %01S %01S %04I') %>"; "0011 011 5 5 0011" |]
        [| "<%= formatDate(d, '%0I %01I %2Y %H %D') %>"; "11 1 19 11 23" |]
        [| "<%= formatDate(d, '%3J %1S %02H %2Y %2W') %>"; "023 5 11 19 04" |]
        [| "<%= formatDate(d, '%B %3Y %03S %1W %2J') %>"; "Jan 019 025 4 23" |]
        [| "<%= formatDate(d, '%1H %I %Y %S %W') %>"; "1 11 2019 25 4" |]
        [| "<%= formatDate(d, '%0I %P %4S %1M %2M') %>"; "11 AM 0025 1 01" |]
        [| "<%= formatDate(d, '%A %03H %3N %S %3N') %>"; "Wed 011 041 25 041" |]
        [| "<%= formatDate(d, '%4D %P %03N %H %A') %>"; "0023 AM 041 11 Wed" |]
        [| "<%= formatDate(d, '%7Y %02H %4W %2Y %A') %>"; "0002019 11 0004 19 Wed" |]
        [| "<%= formatDate(d, '%A %6Y %3M %01S %7Y') %>"; "Wed 002019 001 5 0002019" |]
        [| "<%= formatDate(d, '%6Y %4M %4M %2D %1N') %>"; "002019 0001 0001 23 1" |]
        [| "<%= formatDate(d, '%6Y %2I %Al %Y %1D') %>"; "002019 11 Wednesday 2019 3" |]
        [| "<%= formatDate(d, '%02N %2M %1J %03N %Bl') %>"; "41 01 3 041 January" |]
        [| "<%= formatDate(d, '%02H %D %2M %0I %0S') %>"; "11 23 01 11 25" |]
        [| "<%= formatDate(d, '%3I %0I %03N %1W %2J') %>"; "011 11 041 4 23" |]
        [| "<%= formatDate(d, '%4Y %04I %6Y %S %W') %>"; "2019 0011 002019 25 4" |]
        [| "<%= formatDate(d, '%1I %0H %Bl %1M %4M') %>"; "1 11 January 1 0001" |]
        [| "<%= formatDate(d, '%M %S %3M %2D %03N') %>"; "1 25 001 23 041" |]
        [| "<%= formatDate(d, '%4S %03N %01S %B %4I') %>"; "0025 041 5 Jan 0011" |]
        [| "<%= formatDate(d, '%M %03S %3H %1W %03S') %>"; "1 025 011 4 025" |]
        [| "<%= formatDate(d, '%1Y %4J %02H %A %3N') %>"; "9 0023 11 Wed 041" |]
        [| "<%= formatDate(d, '%04N %4W %S %04N %02N') %>"; "0041 0004 25 0041 41" |]
        [| "<%= formatDate(d, '%1N %02I %2I %Bl %3S') %>"; "1 11 11 January 025" |]
        [| "<%= formatDate(d, '%3J %2J %2Y %1D %1N') %>"; "023 23 19 3 1" |]
        [| "<%= formatDate(d, '%2M %0N %4I %03N %1Y') %>"; "01 41 0011 041 9" |]
        [| "<%= formatDate(d, '%0S %N %2N %2J %4Y') %>"; "25 41 41 23 2019" |]
        [| "<%= formatDate(d, '%0H %0H %3D %Y %2Y') %>"; "11 11 023 2019 19" |]
        [| "<%= formatDate(d, '%1W %3Y %B %3W %2W') %>"; "4 019 Jan 004 04" |]
        [| "<%= formatDate(d, '%01N %7Y %1W %1M %Bl') %>"; "1 0002019 4 1 January" |]
        [| "<%= formatDate(d, '%2J %1I %04N %4H %3Y') %>"; "23 1 0041 0011 019" |]
        [| "<%= formatDate(d, '%B %2W %4D %04H %04H') %>"; "Jan 04 0023 0011 0011" |]
        [| "<%= formatDate(d, '%2I %Bl %1N %2D %1Y') %>"; "11 January 1 23 9" |]
        [| "<%= formatDate(d, '%2D %I %4H %0S %3J') %>"; "23 11 0011 25 023" |]
        [| "<%= formatDate(d, '%J %02H %1J %4D %3Y') %>"; "23 11 3 0023 019" |]
        [| "<%= formatDate(d, '%2M %04I %B %03H %7Y') %>"; "01 0011 Jan 011 0002019" |]
        [| "<%= formatDate(d, '%2M %0S %Y %1N %3J') %>"; "01 25 2019 1 023" |]
        [| "<%= formatDate(d, '%4I %4W %2I %2M %1Y') %>"; "0011 0004 11 01 9" |]
        [| "<%= formatDate(d, '%1I %Al %2W %02N %1N') %>"; "1 Wednesday 04 41 1" |]
        [| "<%= formatDate(d, '%3S %02I %2Y %4Y %01S') %>"; "025 11 19 2019 5" |]
        [| "<%= formatDate(d, '%1I %01S %4D %B %03N') %>"; "1 5 0023 Jan 041" |]
        [| "<%= formatDate(d, '%3H %3W %3M %01I %J') %>"; "011 004 001 1 23" |]
        [| "<%= formatDate(d, '%03N %4S %4W %04H %02S') %>"; "041 0025 0004 0011 25" |]
        [| "<%= formatDate(d, '%3N %0H %A %03N %2M') %>"; "041 11 Wed 041 01" |]
        [| "<%= formatDate(d, '%3H %4Y %01S %4S %4D') %>"; "011 2019 5 0025 0023" |]
        [| "<%= formatDate(d, '%4I %02I %4S %0N %H') %>"; "0011 11 0025 41 11" |]
        [| "<%= formatDate(d, '%0H %4J %1D %1M %P') %>"; "11 0023 3 1 AM" |]
        [| "<%= formatDate(d, '%02S %1N %2W %2Y %3H') %>"; "25 1 04 19 011" |]
        [| "<%= formatDate(d, '%3M %3I %A %D %04H') %>"; "001 011 Wed 23 0011" |]
        [| "<%= formatDate(d, '%2M %4D %1N %2W %4Y') %>"; "01 0023 1 04 2019" |]
        [| "<%= formatDate(d, '%2N %02I %I %2J %2M') %>"; "41 11 11 23 01" |]
        [| "<%= formatDate(d, '%02I %7Y %01I %Y %4H') %>"; "11 0002019 1 2019 0011" |]
        [| "<%= formatDate(d, '%3W %Bl %5Y %1H %4H') %>"; "004 January 02019 1 0011" |]
        [| "<%= formatDate(d, '%0H %S %4I %01N %7Y') %>"; "11 25 0011 1 0002019" |]
        [| "<%= formatDate(d, '%B %1I %Bl %J %7Y') %>"; "Jan 1 January 23 0002019" |]
        [| "<%= formatDate(d, '%2W %3Y %4N %4M %3N') %>"; "04 019 0041 0001 041" |]
        [| "<%= formatDate(d, '%03H %3S %4Y %2N %4M') %>"; "011 025 2019 41 0001" |]
        [| "<%= formatDate(d, '%4S %01H %4N %6Y %01I') %>"; "0025 1 0041 002019 1" |]
        [| "<%= formatDate(d, '%02S %03H %4Y %4N %02I') %>"; "25 011 2019 0041 11" |]
        [| "<%= formatDate(d, '%3J %04I %W %W %1D') %>"; "023 0011 4 4 3" |]
        [| "<%= formatDate(d, '%4S %2S %7Y %2W %Y') %>"; "0025 25 0002019 04 2019" |]
        [| "<%= formatDate(d, '%0I %04H %2W %2J %4N') %>"; "11 0011 04 23 0041" |]
        [| "<%= formatDate(d, '%J %2I %03S %4H %A') %>"; "23 11 025 0011 Wed" |]
        [| "<%= formatDate(d, '%1D %3D %02I %3N %2J') %>"; "3 023 11 041 23" |]
        [| "<%= formatDate(d, '%6Y %B %04I %04H %02H') %>"; "002019 Jan 0011 0011 11" |]
        [| "<%= formatDate(d, '%4S %02I %J %B %4M') %>"; "0025 11 23 Jan 0001" |]
        [| "<%= formatDate(d, '%3J %1D %04S %1J %04S') %>"; "023 3 0025 3 0025" |]
        [| "<%= formatDate(d, '%1M %3S %D %S %03H') %>"; "1 025 23 25 011" |]
        [| "<%= formatDate(d, '%3M %01I %2H %01I %1Y') %>"; "001 1 11 1 9" |]
        [| "<%= formatDate(d, '%1J %2J %S %03N %4H') %>"; "3 23 25 041 0011" |]
        [| "<%= formatDate(d, '%2I %B %2H %4I %3D') %>"; "11 Jan 11 0011 023" |]
        [| "<%= formatDate(d, '%2S %02I %0S %03N %1D') %>"; "25 11 25 041 3" |]
        [| "<%= formatDate(d, '%4Y %2W %4S %1Y %01H') %>"; "2019 04 0025 9 1" |]
        [| "<%= formatDate(d, '%1I %3M %02I %2J %2M') %>"; "1 001 11 23 01" |]
        [| "<%= formatDate(d, '%1I %Al %03N %4I %1Y') %>"; "1 Wednesday 041 0011 9" |]
        [| "<%= formatDate(d, '%03N %1W %Bl %3I %03S') %>"; "041 4 January 011 025" |]
        [| "<%= formatDate(d, '%02I %4N %02S %4S %2H') %>"; "11 0041 25 0025 11" |]
        [| "<%= formatDate(d, '%04I %Y %1J %1I %Bl') %>"; "0011 2019 3 1 January" |]
        [| "<%= formatDate(d, '%H %B %1J %2N %0I') %>"; "11 Jan 3 41 11" |]
        [| "<%= formatDate(d, '%04N %3M %0I %3D %01N') %>"; "0041 001 11 023 1" |]
        [| "<%= formatDate(d, '%Y %1D %4H %4D %3Y') %>"; "2019 3 0011 0023 019" |]
        [| "<%= formatDate(d, '%0I %04I %4S %4J %4J') %>"; "11 0011 0025 0023 0023" |]
        [| "<%= formatDate(d, '%1Y %04H %02I %1W %2S') %>"; "9 0011 11 4 25" |]
        [| "<%= formatDate(d, '%I %1D %7Y %S %03S') %>"; "11 3 0002019 25 025" |]
        [| "<%= formatDate(d, '%04I %1D %B %3I %02I') %>"; "0011 3 Jan 011 11" |]
        [| "<%= formatDate(d, '%03H %01N %Al %1D %03H') %>"; "011 1 Wednesday 3 011" |]
        [| "<%= formatDate(d, '%3D %4M %3Y %02S %4W') %>"; "023 0001 019 25 0004" |]
        [| "<%= formatDate(d, '%4H %04I %02N %2W %1Y') %>"; "0011 0011 41 04 9" |]
        [| "<%= formatDate(d, '%7Y %D %4Y %W %H') %>"; "0002019 23 2019 4 11" |]
        [| "<%= formatDate(d, '%3W %0S %01N %2W %A') %>"; "004 25 1 04 Wed" |]
        [| "<%= formatDate(d, '%0N %3M %01N %2I %1H') %>"; "41 001 1 11 1" |]
        [| "<%= formatDate(d, '%4S %B %4W %3S %P') %>"; "0025 Jan 0004 025 AM" |]
        [| "<%= formatDate(d, '%S %3I %0S %3H %02I') %>"; "25 011 25 011 11" |]
        [| "<%= formatDate(d, '%3I %2I %01N %5Y %1M') %>"; "011 11 1 02019 1" |]
        [| "<%= formatDate(d, '%4W %4J %4D %03S %4S') %>"; "0004 0023 0023 025 0025" |]
        [| "<%= formatDate(d, '%1I %4D %02H %02H %M') %>"; "1 0023 11 11 1" |]
        [| "<%= formatDate(d, '%4M %A %1H %D %04N') %>"; "0001 Wed 1 23 0041" |]
        [| "<%= formatDate(d, '%2N %3Y %2N %1N %Bl') %>"; "41 019 41 1 January" |]
        [| "<%= formatDate(d, '%2H %3M %W %1H %02H') %>"; "11 001 4 1 11" |]
        [| "<%= formatDate(d, '%3N %03I %0S %4J %3S') %>"; "041 011 25 0023 025" |]
        [| "<%= formatDate(d, '%4M %2D %02N %3I %2D') %>"; "0001 23 41 011 23" |]
        [| "<%= formatDate(d, '%D %4I %4S %4M %3W') %>"; "23 0011 0025 0001 004" |]
        [| "<%= formatDate(d, '%4N %04S %4N %0I %4D') %>"; "0041 0025 0041 11 0023" |]
        [| "<%= formatDate(d, '%P %1H %02N %0N %2S') %>"; "AM 1 41 41 25" |]
        [| "<%= formatDate(d, '%J %01S %A %4W %I') %>"; "23 5 Wed 0004 11" |]
        [| "<%= formatDate(d, '%3I %03N %4Y %0N %0H') %>"; "011 041 2019 41 11" |]
        [| "<%= formatDate(d, '%3Y %1J %01S %1J %4Y') %>"; "019 3 5 3 2019" |]
        [| "<%= formatDate(d, '%1J %03I %04S %6Y %1N') %>"; "3 011 0025 002019 1" |]
        [| "<%= formatDate(d, '%2H %3D %B %0S %J') %>"; "11 023 Jan 25 23" |]
        [| "<%= formatDate(d, '%4I %2W %4M %Al %3M') %>"; "0011 04 0001 Wednesday 001" |]
        [| "<%= formatDate(d, '%2J %2S %H %1S %3I') %>"; "23 25 11 5 011" |]
        [| "<%= formatDate(d, '%03S %1D %01S %D %2S') %>"; "025 3 5 23 25" |]
        [| "<%= formatDate(d, '%2J %1J %03H %3I %3Y') %>"; "23 3 011 011 019" |]
        [| "<%= formatDate(d, '%3H %D %02H %02I %0S') %>"; "011 23 11 11 25" |]
        [| "<%= formatDate(d, '%0I %3I %03N %6Y %4M') %>"; "11 011 041 002019 0001" |]
        [| "<%= formatDate(d, '%I %04N %0I %4J %Y') %>"; "11 0041 11 0023 2019" |]
        [| "<%= formatDate(d, '%1M %1D %2Y %B %1S') %>"; "1 3 19 Jan 5" |]
        [| "<%= formatDate(d, '%1H %04H %A %Bl %H') %>"; "1 0011 Wed January 11" |]
        [| "<%= formatDate(d, '%01I %03N %01S %02H %3D') %>"; "1 041 5 11 023" |]
        [| "<%= formatDate(d, '%3S %4W %S %3S %1H') %>"; "025 0004 25 025 1" |]
        [| "<%= formatDate(d, '%0I %3N %01H %01I %02H') %>"; "11 041 1 1 11" |]
        [| "<%= formatDate(d, '%D %2D %0H %1H %Bl') %>"; "23 23 11 1 January" |]
        [| "<%= formatDate(d, '%3D %4S %04H %1H %4H') %>"; "023 0025 0011 1 0011" |]
        [| "<%= formatDate(d, '%02I %2M %01H %3N %2S') %>"; "11 01 1 041 25" |]
        [| "<%= formatDate(d, '%4Y %B %H %2H %4I') %>"; "2019 Jan 11 11 0011" |]
        [| "<%= formatDate(d, '%4N %02N %1J %1W %2S') %>"; "0041 41 3 4 25" |]
        [| "<%= formatDate(d, '%4I %6Y %3M %I %01N') %>"; "0011 002019 001 11 1" |]
        [| "<%= formatDate(d, '%2W %1Y %01N %04S %03N') %>"; "04 9 1 0025 041" |]
        [| "<%= formatDate(d, '%Bl %1D %W %A %Al') %>"; "January 3 4 Wed Wednesday" |]
        [| "<%= formatDate(d, '%3M %3I %N %4Y %3S') %>"; "001 011 41 2019 025" |]
        [| "<%= formatDate(d, '%04H %S %B %A %02H') %>"; "0011 25 Jan Wed 11" |]
        [| "<%= formatDate(d, '%03H %M %7Y %3Y %0N') %>"; "011 1 0002019 019 41" |]
        [| "<%= formatDate(d, '%3J %P %1I %J %04N') %>"; "023 AM 1 23 0041" |]
        [| "<%= formatDate(d, '%H %04H %6Y %01N %04I') %>"; "11 0011 002019 1 0011" |]
        [| "<%= formatDate(d, '%1J %1Y %03I %7Y %P') %>"; "3 9 011 0002019 AM" |]
        [| "<%= formatDate(d, '%1W %02H %4D %1H %2W') %>"; "4 11 0023 1 04" |]
        [| "<%= formatDate(d, '%I %Bl %4J %Bl %M') %>"; "11 January 0023 January 1" |]
        [| "<%= formatDate(d, '%S %1Y %1M %Bl %2I') %>"; "25 9 1 January 11" |]
        [| "<%= formatDate(d, '%3I %2Y %4N %03H %1H') %>"; "011 19 0041 011 1" |]
        [| "<%= formatDate(d, '%03I %03S %J %2I %3W') %>"; "011 025 23 11 004" |]
        [| "<%= formatDate(d, '%03H %4I %4Y %N %2I') %>"; "011 0011 2019 41 11" |]
        [| "<%= formatDate(d, '%02I %1I %B %01I %3I') %>"; "11 1 Jan 1 011" |]
        [| "<%= formatDate(d, '%A %02I %04S %Bl %04H') %>"; "Wed 11 0025 January 0011" |]
        [| "<%= formatDate(d, '%4I %2H %03H %3Y %5Y') %>"; "0011 11 011 019 02019" |]
        [| "<%= formatDate(d, '%3H %02S %1Y %03H %Al') %>"; "011 25 9 011 Wednesday" |]
        [| "<%= formatDate(d, '%2M %03H %3D %04S %0I') %>"; "01 011 023 0025 11" |]
        [| "<%= formatDate(d, '%B %4S %4S %0I %02N') %>"; "Jan 0025 0025 11 41" |]
        [| "<%= formatDate(d, '%Y %2D %1Y %1S %3D') %>"; "2019 23 9 5 023" |]
        [| "<%= formatDate(d, '%4I %2W %1H %S %01I') %>"; "0011 04 1 25 1" |]
        [| "<%= formatDate(d, '%A %2D %01N %5Y %03S') %>"; "Wed 23 1 02019 025" |]
        [| "<%= formatDate(d, '%1H %4M %M %3I %03N') %>"; "1 0001 1 011 041" |]
        [| "<%= formatDate(d, '%4J %M %0S %3M %W') %>"; "0023 1 25 001 4" |]
        [| "<%= formatDate(d, '%2D %01H %03H %Al %0I') %>"; "23 1 011 Wednesday 11" |]
        [| "<%= formatDate(d, '%2H %4M %2M %2J %03I') %>"; "11 0001 01 23 011" |]
        [| "<%= formatDate(d, '%I %I %3M %4S %1N') %>"; "11 11 001 0025 1" |]
        [| "<%= formatDate(d, '%03I %01N %2H %2Y %A') %>"; "011 1 11 19 Wed" |]
        [| "<%= formatDate(d, '%04I %4N %H %A %3D') %>"; "0011 0041 11 Wed 023" |]
        [| "<%= formatDate(d, '%3M %01I %1J %03N %H') %>"; "001 1 3 041 11" |]
        [| "<%= formatDate(d, '%J %04I %4I %Y %A') %>"; "23 0011 0011 2019 Wed" |]
        [| "<%= formatDate(d, '%01H %2N %H %04I %2D') %>"; "1 41 11 0011 23" |]
        [| "<%= formatDate(d, '%4J %01N %4D %01N %2M') %>"; "0023 1 0023 1 01" |]
        [| "<%= formatDate(d, '%4W %A %04N %03S %04H') %>"; "0004 Wed 0041 025 0011" |]
        [| "<%= formatDate(d, '%3W %04H %03S %1M %J') %>"; "004 0011 025 1 23" |]
        [| "<%= formatDate(d, '%2J %Al %02H %Y %3S') %>"; "23 Wednesday 11 2019 025" |]
        [| "<%= formatDate(d, '%3W %2D %3W %0I %5Y') %>"; "004 23 004 11 02019" |]
        [| "<%= formatDate(d, '%4W %Y %3N %3S %03I') %>"; "0004 2019 041 025 011" |]
        [| "<%= formatDate(d, '%02S %2J %4S %4S %4M') %>"; "25 23 0025 0025 0001" |]
        [| "<%= formatDate(d, '%04S %2M %0N %2M %03H') %>"; "0025 01 41 01 011" |]
        [| "<%= formatDate(d, '%2M %03N %3N %3I %04H') %>"; "01 041 041 011 0011" |]
        [| "<%= formatDate(d, '%4D %2N %4W %4J %0N') %>"; "0023 41 0004 0023 41" |]
        [| "<%= formatDate(d, '%H %03I %03H %3N %3H') %>"; "11 011 011 041 011" |]
        [| "<%= formatDate(d, '%3I %0S %1W %02N %02N') %>"; "011 25 4 41 41" |]
    |]

    let private globals = seq {
        yield! defaultGlobals
        yield """
        const tzOffset = (- new Date().getTimezoneOffset()) / 60;
        const tzString = `${ tzOffset > 0 ? '+' : '' }${tzOffset}`;
        const d = new Date(`Wed Jan 23 2019 11:41:25 GMT${tzString}`);
        """
        }

    [<Test; TestCaseSource("formatDateFunctionSpec")>]
    let ``'formatDate' function complies with Neo's functionality`` templateString expected =
        renderTemplate globals emptyIncludes emptyContext templateString
        |> shouldEqual expected
    
