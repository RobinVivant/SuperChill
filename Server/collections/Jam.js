

Jam = new Meteor.Collection('jam');

Jam.allow({
    insert: function (userId, doc) {
        doc.leapTargets = [];
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
        return Jam.find({});
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
        TrackGroups.update({
                jamId: doc.jamId,
                tracks: doc._id
            },{
                $pull :{
                    tracks: doc._id
                }
            },
            { multi: true }
        );

        TrackGroups.remove({
                jamId: doc.jamId,
                tracks: {$size:0}
            }
        );

        return true;
    }
});
if (Meteor.isServer) {
    JamTracks._ensureIndex({_id: 1});

    Meteor.publish('jam-tracks', function (jamId) {
        return JamTracks.find({jamId: jamId});
    });
}
