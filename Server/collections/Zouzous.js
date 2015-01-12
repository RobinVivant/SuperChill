

Zouzous = new Meteor.Collection('zouzous');
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
    Zouzous._ensureIndex({_id: 1});

    Meteor.publish('zouzou', function (hexId) {
        return Zouzous.find({hexId: hexId});
    });
    Meteor.publish('zouzouList', function () {
        return Zouzous.find({});
    });
}

