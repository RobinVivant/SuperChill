

TrackGroups = new Meteor.Collection('track-groups');

TrackGroups.allow({
    insert: function (userId, doc) {
        if( !doc.effects ){
            doc.effects = [
                {name: 'volume', value: 1},
                {name: 'gargle', value: 0},
                {name: 'echo', value: 0},
                {name: 'reverb', value: 0},
                {name: 'flanger', value: 0},
                {name: 'chorus', value: 0}
            ];

            doc.leapGesturesMapping = {
                x: [],
                y: [],
                pitch: []
            }
        }
        return true;
    },
    update: function (userId, doc, fields, modifier) {
        return true;
    },
    remove: function (userId, doc) {
        Jam.update({
            _id:doc.jamId
        },{
            $pull:{
                leapTargets : doc._id
            }
        });
        return true;
    }
});

if (Meteor.isServer) {

    TrackGroups._ensureIndex({jamId: 1});

    Meteor.publish('track-groups', function (jamId) {
        return TrackGroups.find({jamId: jamId});
    });

    Meteor.methods({
       'updateGroupEffect': function(groupId, effectName, percentValue){
           TrackGroups.update({
                   _id:groupId,
                   'effects.name' : effectName
               }, {
                   $set : {
                       'effects.$.value' : percentValue
                   }
               }
           );
       },
        'removeGroupTrack': function(groupId, trackId){
            console.log(arguments);
            TrackGroups.update({
                    _id:groupId
                }, {
                    $pull : {
                        tracks : trackId
                    }
                }, function(){
                    console.log(arguments);
                }
            );
        }

    });
}