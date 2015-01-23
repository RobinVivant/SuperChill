

TrackGroups = new Meteor.Collection('track-groups');

TrackGroups.allow({
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

    TrackGroups._ensureIndex({jamId: 1});

    Meteor.publish('track-groups', function (jamId) {
        return TrackGroups.find({jamId: jamId});
    });

}