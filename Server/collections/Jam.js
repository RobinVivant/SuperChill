

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
