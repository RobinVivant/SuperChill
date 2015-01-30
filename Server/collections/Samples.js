
Samples = new Meteor.Collection('samples');


Samples.allow({
    insert: function (userId, doc) {
        return true;
    },
    update: function (userId, doc, fields, modifier) {
        return true;
    },
    remove: function (userId, doc) {
        return true;
    }
});

if (Meteor.isServer) {

    Samples._ensureIndex({_id: 1});

    Meteor.publish('samples', function () {
        return Samples.find();
    });

    Meteor.startup(function(){
        var fs = Npm.require('fs');

        var isLocal = Meteor.absoluteUrl({replaceLocalhost: true}).indexOf("127.0.0.1") > -1;

        function scanSamples(dir){
            var samples = [];
            var files = fs.readdirSync(dir);
            for(var i in files){
                if (!files.hasOwnProperty(i)) continue;
                var name = dir+'/'+files[i];
                var finalName = files[i]
                    .replace(/_/g, ' ')
                    .replace(/\..*$/g, ' ')
                    .replace(/120BPM/g, '')
                    .replace(/[0-9]*/g, '')
                    .trim();
                if (fs.statSync(name).isDirectory() && name[0] != '.' ){
                    samples.push({
                        name: finalName,
                        childs: scanSamples(name)
                    });
                } else {
                    samples.push({
                        name: finalName,
                        path: name.substring((isLocal ? process.env.PWD+"/public" : __meteor_bootstrap__.serverDir+'/../web.browser/app').length)
                    });
                }
            }
            return samples;
        }
        Samples.remove({});

        if (isLocal) {
            /**
             * LOCAL (DEV)
             */
            Samples.insert({
                name: "loops",
                childs: scanSamples(process.env.PWD+'/public/loops')
            });
        }else{
            Samples.insert({
                name: "loops",
                childs: scanSamples(__meteor_bootstrap__.serverDir+'/../web.browser/app/loops')
            });
        }


    });



}