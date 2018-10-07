var nextEventListStart = 0;
var isListtoBottom = false;
var timerEventObject = function (eventId, validTime, postTime) {
    this.eventId = eventId;
    this.str_el = '.eventItem[data-eventid="' + eventId + '"]';
    this.validTime = validTime;
    this.postTime = postTime;
    this.isEnd = false;
};

var getDistanceString = function (nowTime, item) {
    let distance = item.validTime.getTime() - nowTime.getTime();
    let wholeWidth = item.validTime.getTime() - item.postTime.getTime();
    let hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    let minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    let seconds = Math.floor((distance % (1000 * 60)) / 1000);
    let str = "";
    if (hours > 0)
        str += hours + ':';
    if (minutes < 10)
        str += '0';
    str += minutes + ':';
    if (seconds < 10)
        str += '0';
    str += seconds;
    if (distance < 0) {
        distance = 0;
        item.isEnd = true;
        str = "END";
    }
    return [str, (distance / wholeWidth) * 100];
}
var timerEventQueue = [];

function createEventEl(eventId, eventName, postType, postTime, validTime, sourceFrom) {
    let modifypostTime = new Date(postTime);
    let modifyvalidTime = new Date(validTime);
    let str_HTML = '<div class="eventItem waves-effect" data-eventid="' + eventId + '" onclick="seeEventDetail(' + eventId + ')">' +
        '<div class="eventType">' + modifyselfPostType(postType) + '</div>' +
        '<div class="eventName">' + eventName + '</div>' +
        '<div class="eventPostTime">' + modifyselfTimeStyle(modifypostTime) + '</div>';
    if (postType == "timer") {
        timerEventQueue.push(new timerEventObject(eventId, modifyvalidTime, modifypostTime));
        str_HTML += '<div class="eventValidTime"></div>';
        str_HTML += '<div class="eventProgress"></div>';
    }
    str_HTML += '</div>';
    // sourceFrom= 0:Web API, 1:SignalR
    if (sourceFrom === 0)
        $(".eventList").append(str_HTML);
    else {
        if (sourceFrom === 1)
            $(".eventList").prepend(str_HTML);
    }
}

function modifyselfTimeStyle(inputdate) {
    let result = inputdate.getFullYear() + "/" + inputdate.getMonth() + "/" + inputdate.getDate() + " ";
    if (inputdate.getHours() < 10)
        result += "0";
    result += inputdate.getHours() + ":";
    if (inputdate.getMinutes() < 10)
        result += "0";
    result += inputdate.getMinutes();
    return result;
}

function modifyselfPostType(str) {
    switch (str) {
        case "timer":
            return '<div class="timer">限時</div>';
        default:
            return '<div class="normal">一般</div>';
    }
}