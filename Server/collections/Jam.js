

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
}

JamTracks = new Meteor.Collection('jam-tracks');
JamTracks.allow({
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
    JamTracks._ensureIndex({_id: 1});

    Meteor.publish('jam-tracks', function (jamId) {
        return JamTracks.find({jamId: jamId});
    });
}

if (Meteor.isServer){
    Meteor.startup(function () {
        //if( Jam.find().length == 0) {
        JamTracks.remove({});
        Jam.remove({});
        Jam.insert({
            name: "Jam 1"
        });
        Jam.insert({
            name: "Jam 2"
        });
        Jam.insert({
            name: "Jam 3"
        });
        Jam.insert({
            name: "Jam 4"
        });
        Jam.insert({
            name: "Jam 5"
        });
        Jam.insert({
            name: "Jam 6"
        });
        Jam.insert({
            name: "Jam 7"
        });
        Jam.insert({
            name: "Jam 8"
        });
        //}
    });
}