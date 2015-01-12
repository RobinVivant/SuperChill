

Zouzous = new Meteor.Collection('zouzou');
Zouzous.allow({
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
    Zouzous._ensureIndex({hexId: 1});

    Meteor.publish('zouzou', function (hexId) {
        return Zouzous.find({hexId: hexId});
    });
}