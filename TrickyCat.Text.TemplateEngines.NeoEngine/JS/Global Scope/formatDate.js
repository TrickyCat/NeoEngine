const formatDate = (() => {
    const shortDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    const longDays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const shortMonths = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const longMonths = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];

    const pad = (dataString, length, padString = "0") => {
        const padLength = Math.abs(length - dataString.length);
        return `${padString.repeat(padLength)}${dataString}`;
    };

    const dayOfYear = date => {
        const start = new Date(date.getFullYear(), 0, 0);
        const diff = (date - start) + ((start.getTimezoneOffset() - date.getTimezoneOffset()) * 60 * 1000);
        const oneDay = 1000 * 60 * 60 * 24;
        const day = Math.floor(diff / oneDay);
        return day;
    };
    
    const get24Hour = date => date.getHours();

    const fitLength = (resultStrFn, supportsLength, length) => {
        const resultString = resultStrFn();
        const useDefaultLength = length === undefined;

        if (!supportsLength || useDefaultLength) {
            return resultString;
        }
        if (resultString.length < length) {
            return pad(resultString, length);
        }
        return resultString.substr(resultString.length - length);
    };

    const fl = (resultStrFn, length) => fitLength(resultStrFn, true, length);

    const Y = (date, length) => fl(() => `${date.getFullYear()}`, length);

    const M = (date, length) => fl(() => `${date.getMonth() + 1}`, length);

    const D = (date, length) => fl(() => `${date.getDate()}`, length);

    const J = (date, length) => fl(() => `${dayOfYear(date)}`, length);

    const W = (date, length) => fl(() => `${Math.floor(dayOfYear(date) / 7) + 1}`, length);

    const H = (date, length) => fl(() => `${get24Hour(date)}`, length);

    const I = (date, length) => fl(() => {
        const hour24 = get24Hour(date);
        const rawHour12 = hour24 <= 11 ? hour24 : hour24 - 12;
        const hour12 = rawHour12 === 0 ? 12 : rawHour12;
        return `${hour12}`;
    }, length);

    const N = (date, length) => fl(() => `${date.getMinutes()}`, length);

    const S = (date, length) => fl(() => `${date.getSeconds()}`, length);

    const P = (date) => {
        const hour24 = get24Hour(date);
        return (hour24 < 12 ? 'AM' : 'PM');
    };

    const A = (date, useLongName) => {
        const dayOfWeekIdx = date.getDay();
        return (useLongName
            ? longDays[dayOfWeekIdx]
            : shortDays[dayOfWeekIdx]);
    };

    const B = (date, useLongName) => {
        const monthIdx = date.getMonth();
        return (useLongName
            ? longMonths[monthIdx]
            : shortMonths[monthIdx]);
    };

    const handler = (formatHandler, regEx, argsProvider, date, str) => {
        const execRes = regEx.exec(str);
        if (execRes) {
            const matchedStr = execRes[0];
            const formatResult = formatHandler(date, ...argsProvider(execRes));

            return { ok: true, data: str.replace(matchedStr, formatResult)};
        }
        return { ok: false, data: str};
    };

    const handlerNum = (formatHandler, regEx, date, str) =>
        handler(formatHandler, regEx, execRes => {
            const parsedNumberSpecifier = execRes[1] !== undefined
                ? Number(execRes[1])
                : undefined;
            const numberSpecifier = parsedNumberSpecifier === 0 ? undefined : parsedNumberSpecifier;
            return [numberSpecifier];
        }, date, str);


    const formats = [
        {
            tag: 'Y - Year',
            regEx: /%(\d*)Y/,
            handle: function(date, str) { return handlerNum(Y, this.regEx, date, str); }
        },
        {
            tag: 'M - Month of the year (1-12)',
            regEx: /%(\d*)M/,
            handle: function(date, str) { return handlerNum(M, this.regEx, date, str); }
        },
        {
            tag: 'D - Day of the month (1-31)',
            regEx: /%(\d*)D/,
            handle: function(date, str) { return handlerNum(D, this.regEx, date, str); }
        },
        {
            tag: 'J - Day of the year (1-366)',
            regEx: /%(\d*)J/,
            handle: function(date, str) { return handlerNum(J, this.regEx, date, str); }
        },
        {
            tag: 'W - Week of the year',
            regEx: /%(\d*)W/,
            handle: function(date, str) { return handlerNum(W, this.regEx, date, str); }
        },
        {
            tag: 'H - Hour (0-23)',
            regEx: /%(\d*)H/,
            handle: function(date, str) { return handlerNum(H, this.regEx, date, str); }
        },
        {
            tag: 'I - Hour (1-12)',
            regEx: /%(\d*)I/,
            handle: function(date, str) { return handlerNum(I, this.regEx, date, str); }
        },
        {
            tag: 'N - Minutes (0-59)',
            regEx: /%(\d*)N/,
            handle: function(date, str) { return handlerNum(N, this.regEx, date, str); }
        },
        {
            tag: 'S - Seconds (0-59)',
            regEx: /%(\d*)S/,
            handle: function(date, str) { return handlerNum(S, this.regEx, date, str); }
        },
        {
            tag: 'P - AM/PM',
            regEx: /%P/,
            handle: function(date, str) { return handler(P, this.regEx, _ => [], date, str); }
        },
        {
            tag: 'A - Day name',
            regEx: /%A([il]?)/,
            handle: function(date, str) { return handler(A, this.regEx, execRes => [execRes[1] !== undefined ? execRes[1] === 'l' : undefined], date, str); }
        },
        {
            tag: 'B - Month name',
            regEx: /%B([il]?)/,
            handle: function(date, str) { return handler(B, this.regEx, execRes => [execRes[1] !== undefined ? execRes[1] === 'l' : undefined], date, str); }
        }
    ];

    const formatter = (date, formatString) => {
        const formattedString = formats.reduce((acc, entry) => {
            let handledStr = acc;
            let handled = false;

            do {
                const { ok, data } = entry.handle(date, handledStr);
                handled = ok;
                handledStr = data;
            } while (handled);

            return handledStr;
        }, formatString);

        return formattedString;
    };

    return formatter;
})();
