function docOnlookSend(name, type, fileData, destIP, successHandler, errorHandler) { // Pack the file and send
    if ($) {  // Bail out if jQuery library missing.
        dataJson = { "name": name, "type": type, "data": fileData, 'action':'SEND_FILE' };
        $.ajax({
            'data': dataJson,
            'dataType': 'json',
            'success': function (response) {
                successHandler(response);
            },
            'error': function (response) {
                errorHandler(response);
            },
            'url': 'http://' + destIP + ':2112',
            'type': 'post',
            'crossDomain': 'true',
        });
    }
}

function docOnlookFind(destIP,successHandler, errorHandler){ // Finding the doc-onlook running device on local network.
    if ($) {  // Bail out if jQuery library missing.
        dataJson = { 'action':'FIND_DEVICE' };
        $.ajax({
            'data': dataJson,
            'dataType': 'json',
            'success': function (response) {
                successHandler(response);
            },
            'error': function (response) {
                errorHandler(response);
            },
            'url': 'http://' + destIP + ':2112',
            'type': 'post',
            'crossDomain': 'true',
        });
    }   
}