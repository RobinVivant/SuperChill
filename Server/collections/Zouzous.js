

Zouzous = new Meteor.Collection('zouzous');
Zouzous.allow({
    insert: function (userId, doc) {
/*
        var zouzous = Zouzous.find({jamId: doc.jamId}).fetch();
        if( zouzous.length == 0) {
            var c = hslToRgb(Random.fraction(), 1, 0.3);
            doc.hexId = rgbToHex(c[0], c[1], c[2]).substring(1);
            return true;
        }

        var zzHue = [];

        for(var i in zouzous ){
            var rgb = hexToRgb('#'+zouzous[i].hexId);
            var hue = rgbToHsl(rgb.r, rgb.g, rgb.b)[0];
            zzHue.push(hue);
        }


        var keys = zzHue.sort();
        var index = keys.length-1;
        var distance = Math.abs(1-keys[index]);
        for( var i in keys){
            if( i == 0 ){
                continue;
            }
            var d = Math.abs(keys[i-1]-keys[i]);

            if( d >= distance ){
                distance = d;
                index = i-1;
            }
        }
        var c = hslToRgb((keys[index] + (distance/2 || 0.5)) % 1, 1, 0.3);
*/
        var c;

        c = hslToRgb(Random.fraction(), 1, 0.3);
        //c = rgbToHex(c[0], c[1], c[2]).substring(1);

        doc.hexId = rgbToHex(c[0], c[1], c[2]).substring(1);

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

    Zouzous._ensureIndex({hexId: 1, jamId:1});

    Meteor.publish('zouzou', function (hexId) {
        return Zouzous.find({hexId: hexId});
    });
    Meteor.publish('zouzouList', function (jamId) {
        return Zouzous.find({});
    });

}

