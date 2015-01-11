

Jam = new Meteor.Collection('jam');


Jam.allow({
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

    Jam._ensureIndex({_id: 1});

    Meteor.publish('jam', function (jamId) {
        var ret = Jam.find({_id: jamId});

        if( !ret.fetch().length ) {
            throw new Meteor.Error(500, "Jam not found !");
        }
        return Jam.find({_id: jamId});
    });

    Meteor.publish('jamList', function () {
        return Jam.find({},{fields:{
            name:1,
            _id:1
        }});
    });

    Meteor.startup(function(){
        if( Jam.find().length == 0) {
            Jam.remove({});
            console.log(Jam.insert({
                name: "YOLO JAM !",
                zouzous: [],
                tracks: []
            }));
        }
    });



}