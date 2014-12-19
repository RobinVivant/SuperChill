

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

    Samples._ensureIndex({id: 1});

    Meteor.publish('samples', function () {
        return Samples.find();
    });

    if( Samples.find().count() == 0 ){
        Samples.insert({
            title:"Yolo song",
            duration: 120
        });
    }
}