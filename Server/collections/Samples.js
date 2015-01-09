

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
        function scanSamples(dir){
            var samples = [];
            var files = fs.readdirSync(dir);
            for(var i in files){
                if (!files.hasOwnProperty(i)) continue;
                var name = dir+'/'+files[i];
                var finalName = files[i].replace(/_/g, ' ').replace(/\..*$/g, ' ');
                if (fs.statSync(name).isDirectory()){
                    samples.push({
                        name: finalName,
                        childs: scanSamples(name)
                    });
                } else {
                    samples.push({
                        name: finalName,
                        path: name.substring((process.env.PWD+"/public").length)
                    });
                }
            }
            return samples;
        }
        Samples.remove({});
        Samples.insert({
            name: "loops",
            childs: scanSamples(process.env.PWD+'/public/loops')
        });

    });



}