var TranslateServiceClass = new function (e) {
    var self = this;





    const express = require('express');
    const fs = require('fs');

    var https = require('https');
    var privateKey = fs.readFileSync('ssl/private.key', 'utf8');
    var certificate = fs.readFileSync('ssl/domain.crt', 'utf8');
    var credentials = { key: privateKey, cert: certificate };
    const app = express();
    var httpsServer = https.createServer(credentials, app);
    self.bodyParser = null;
    self.multer = null;
    self.upload = null;
    self.InitialService = function (e) {
        self.bodyParser = require('body-parser');
        self.multer = require('multer'); // v1.0.5
        self.upload = self.multer({
            limits: { fieldSize: 25 * 1024 * 1024 }
        });


        // for parsing multipart/form-data
        // app.use(bodyParser.json()); // for parsing application/json

        app.use(self.bodyParser.urlencoded({ extended: true })); // for parsing application/x-www-form-urlencoded


        app.get('/', function (req, res) {
            res.send('Hello World!');
        });

        //app.get('/TranslateServiceSimple/:msg', function (req, res) {


        //    translate(req.params.msg, { from: 'fa', to: "en" }).then(x => {
        //        res.send(x);
        //    }).catch(err => {
        //        console.error(err);
        //    });
        //});


        //
        //	سرویس اصلی 
        //
        app.post('/VideoService', self.upload.array(), function (req, res, next) {


            if (req.body === undefined)
                res.status(500).send("Can not be null");
            else {


                res.send(req.body);

                fs.appendFile(__dirname + '\\nodeJsService.txt', "from " + req.body, null);
                fs.appendFile(__dirname + '\\nodeJsService.txt', err.toString() + "\r\n", null);
            }

            // res.json(req.body);
        });




        // شروع سرور به پورت 
        // 8734
        //app.listen(81, function () {
        //    fs.appendFile(__dirname + '\\nodeJsService.txt', 'App listening on port 9120' + "\r\n", null);
        //    console.log('App listening on port 8734');
        //});
        httpsServer.listen(443, '192.168.1.111', function () {
            fs.appendFile(__dirname + '\\nodeJsService.txt', 'App listening on port 9121' + "\r\n", null);
            console.log('App listening on port 8734');
        });

    }

    self.InitialService();
}









