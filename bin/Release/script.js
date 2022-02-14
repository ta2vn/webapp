function ajax(url, success, fall) {
    var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');
    xhr.open('GET', url);
    xhr.overrideMimeType("text/html");
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && xhr.status == 200) {
            if (success) success(xhr.responseText)
        }
        else {
            if (xhr.readyState > 3 && xhr.status >= 400) {
                if (fall) fall(xhr.responseText)
            }
        }
    };
    xhr.setRequestHeader("Content-Type", "text/plain;charset=UTF-8");
    xhr.send();
    return xhr;
}

function ajax1(method, url, data, success, fall) {
    var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP")

    xhr.open(method, url)
    xhr.overrideMimeType("text/html")
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && (xhr.status == 200 || xhr.status == 201)) {
            if (success) success(xhr.responseText)
        }
        else {
            if (xhr.readyState > 3 && xhr.status >= 400) {
                
                if (fall) fall(xhr.responseText)
            }
        }

    };
    xhr.setRequestHeader("Content-Type", "text/plain;charset=UTF-8");
    if (data == undefined) {
        xhr.send();
    } else {
        xhr.send(data);
    }
    return xhr;
}

function ajax1a(method, url, data, success, fall) {
    var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP")

    xhr.open(method, url)
    xhr.overrideMimeType("text/html")
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && (xhr.status == 200 || xhr.status == 201)) {
            let item = parserData(xhr.responseText)
            if (success) success(item)
        }
        else {
            if (xhr.readyState > 3 && xhr.status >= 400) {
                let error = parserData(xhr.responseText)
                if (fall) fall(error)
            }
        }

    };
    xhr.setRequestHeader("Content-Type", "text/plain;charset=UTF-8");
    if (data == undefined) {
        xhr.send();
    } else {
        xhr.send(JSON.stringify(fixDate(data)));
    }
    return xhr;
}

function parserData(str) {
    if (str.length > 0) {
        if (str.charAt(0) == "{" || str.charAt(0) == "[") {
            return JSON.parse(str, JSON.dateParser);
        }
    }
    return str
}

if (window.JSON && !window.JSON.dateParser) {
    JSON.dateParser = function (k, v) {
        if (typeof v === 'string' && (v.startsWith("Date(") || v.startsWith("DateTime(")) && v.endsWith(")")) {
            return toDate(v);
        }
        return v;
    };
}