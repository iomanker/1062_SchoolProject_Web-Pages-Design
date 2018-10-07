var randomText = function (selector, opt) {
    var that = this;
    // var time = 0;
    if (typeof selector != "undefined")
        this.$el = $(selector);
    // about time
    this.now;
    this.then = Date.now();
    this.delta;
    this.currentTimeOffset = 0;

    // about current words
    this.currentWords = null;
    this.currentChar = null;

    // about all options setup
    var options = {
        fps: 30,
        timeOffset: 5,
        textColor: '#000',
        fontSize: '50px',
        needUpdate: true
    };
    if (typeof opt != "undefined") {
        for (key in opt) {
            options[key] = opt[key];
        }
    }
    this.needUpdate = true;
    this.fps = options.fps;
    this.interval = 1000 / this.fps;
    this.timeOffset = options.timeOffset;
    this.textColor = options.textColor;
    this.fontSize = options.fontSize;
    this.chars = [
      '!', '§', '$', '%',
      '&', '/', '(', ')',
      '=', '?', '_', '<',
      '>', '^', '°', '*',
      '#', '-', ':', ';', '~'
    ];
    function update() {
        if (that.needUpdate) {
            that.updateSentence();
        }
        requestAnimationFrame(update);
    };
    this.mainStart(0);
    update();
};

randomText.prototype.modifyTexts = function (texts) {
    this.texts = texts;
}

randomText.prototype.getRandomChar = function (characterToReplace) {
    if (characterToReplace == " ") {
        return ' ';
    }
    var randNum = Math.floor(Math.random() * this.chars.length);
    var lowChoice = -.5 + Math.random();
    var pickedUpChar = this.chars[randNum];
    return pickedUpChar;
};

randomText.prototype.updateSentence = function () {
    this.now = Date.now();
    this.delta = this.now - this.then;
    if (this.delta > this.interval) {
        if (this.currentWords == "") {
            return;
        }
        ++this.currentTimeOffset;
        var sentence = "";
        if (this.currentTimeOffset === this.timeOffset
         && this.currentChar < this.currentWords.length) {
            ++this.currentChar;
            this.currentTimeOffset = 0;
        }
        for (var k = 0; k < this.currentChar; ++k) {
            sentence += this.currentWords[k];
        }
        for (var k = 0; k < this.currentWords.length - this.currentChar; ++k) {
            sentence += this.getRandomChar(this.currentWords[this.currentChar + k]);
        }

        if (this.currentChar >= this.currentWords.length) {
            this.needUpdate = false;
        }
        this.$el.html("<span>" + sentence + "</span>");
        this.then = this.now - (this.delta % this.interval);
    }
};

randomText.prototype.restart = function () {
    this.currentChar = 0;
    this.needUpdate = true;
};

randomText.prototype.mainStart = function (texts) {
    this.currentWords = texts;
    this.restart();
}