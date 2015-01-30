
Session.setDefault('tracksToGroup', {});

function isLeapMotionActivated(id){
    return _.contains(Jam.findOne({_id:Session.get('jamId')}).leapTargets, id)
};

Template.JamTablet.helpers({
    tracks: function () {
        return JamTracks.find();
    },
    zouzous: function(){
        return Zouzous.find({jamId: Session.get("jamId")});
    },
    trackName: function(path){
        return path.replace(/_/g, ' ')
            .replace(/\..*$/g, ' ')
            .replace(/120BPM/g, '')
            .replace(/[0-9]*/g, '')
            .trim()
            .replace(/(\/.*\/)*/g, '');
    },
    verticalSectionHeight: function(){
        return $(window).height()-160+'px';
    },
    checkedTrack: function(){
        var tracks = Session.get('tracksToGroup');
        return tracks[this._id] ? "checkedTrack" : "";
    },
    tracksGroups: function(){
        return TrackGroups.find().fetch();//Session.get("tracksGroups");
    },
    isGroupSelected: function(){
        if( Session.get('selectedGroup') === this._id )
            return 'trackGroupSelected';
    },
    getSelectedGroupColor: function(){
        var group = TrackGroups.findOne({_id:Session.get('selectedGroup')});
        if( group )
            return group.color;
    },
    shouldDisplayEffects: function(){
        return Session.get('selectedGroup') ? true : false;
    },
    currentEffects: function(){
        var group = TrackGroups.findOne({_id:Session.get('selectedGroup')});
        if( group ){
            return group.effects;
        }
    },
    effectToPercent: function(value){
        return value * 100;
    },
    isLeapMotionActivated: function(id){
        return isLeapMotionActivated(id);
    },
    getMagicValue: function(){
        return isLeapMotionActivated(Session.get('selectedGroup')) ? 'ON' : 'OFF';
    }
});

var updateEffectTimeout;
function updateEffect(name, e, clientX){
    var percent = (clientX-32 - $(e.currentTarget).position().left) / $(e.currentTarget).width();

    if( percent > 1 || percent < 0 ){
        return;
    }

    $(e.currentTarget).find('.slider-bar').css('width', (percent*100)+'%');

    clearTimeout(updateEffectTimeout);
    updateEffectTimeout = setTimeout(function(){
        Meteor.call('updateGroupEffect', Session.get('selectedGroup'), name, percent );
    }, 200);
}

var adjustingEffect = false;

Template.JamTablet.events({
    'click .leftPanel .trackItem': function(e, tmpl){
        var trs = Session.get('tracksToGroup');
        if(trs[this._id]){
            trs[this._id] = undefined;
            if( Object.keys(trs).length === 1 ){
                $('.createGroupButton').velocity('stop').velocity({
                    properties:{
                        scale: [0, 1],
                        opacity: [0, 1]
                    },options:{
                        duration: 200,
                        complete: function(){
                            $('.createGroupButton').hide();
                        }
                    }
                });
            }
        }else{
            if( Object.keys(trs).length === 0 ){
                $('.createGroupButton').show();
                $('.createGroupButton').css('bottom', ($('.leftPanel').height()-50)/2 +'px');
                $('.createGroupButton').css('left', $('.leftPanel').width()-35 +'px');
                $('.createGroupButton').velocity('stop').velocity({
                    properties:{
                        scale: [1, 0],
                        opacity: [1, 0]
                    },options:{
                        duration: 200
                    }
                });
            }
            trs[this._id] = true;
        }
        Session.set('tracksToGroup', trs);
    },
    'click .createGroupButton': function(e, tmpl){
        //var grps = TrackGroups.find().fetch();

        var c;

        c = hslToRgb(Random.fraction(), 1, 0.3);
        c = rgbToHex(c[0], c[1], c[2]).substring(1);
        /*
         if( grps.length == 0) {
         c = hslToRgb(Random.fraction()%0.75, 1, 0.3);
         c = rgbToHex(c[0], c[1], c[2]).substring(1);
         }else{

         var zzHue = [];

         for(var i in grps ){
         var rgb = hexToRgb('#'+grps[i].color);
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

         c = hslToRgb((keys[index] + (distance/2 || 0.5)) % 0.75, 1, 0.3);
         c = rgbToHex(c[0], c[1], c[2]).substring(1);
         }
         */
        TrackGroups.insert({
            color: c,
            name: chance.first(),
            jamId: Session.get('jamId'),
            tracks: Object.keys(Session.get('tracksToGroup'))
        });

        Session.set('tracksToGroup', {});
        $('.createGroupButton').velocity('stop').velocity({
            properties:{
                scale: [0, 1],
                opacity: [0, 1]
            },options:{
                duration: 200,
                complete: function(){
                    $('.createGroupButton').hide();
                }
            }
        })
    },
    'click .groupName': function(){
        if(this._id == Session.get('selectedGroup')){
            Session.set('selectedGroup', null);
        }else{
            Session.set('selectedGroup', this._id);
        }
    },
    'click .trackGroup > .fa-trash-o': function(e, tpl){
        var that = this;
        Session.set('selectedGroup', null);
        $(e.currentTarget).parent().velocity({
            properties:{
                scale: [0, 1],
                opacity: [0, 1]
            },options:{
                duration: 200,
                complete: function(){
                    TrackGroups.remove({_id: that._id});
                }
            }
        })
    },
    'touchmove .effect-slider': function(e, tmpl){
        updateEffect(this.name, e, e.originalEvent.changedTouches[0].clientX);
    },
    'touchstart .effect-slider': function(e, tmpl){
        updateEffect(this.name, e, e.originalEvent.changedTouches[0].clientX);
    },
    'mousemove .effect-slider': function(e, tmpl){
        if( adjustingEffect ) {
            updateEffect(this.name, e, e.originalEvent.clientX);
        }
    },
    'mousedown .effect-slider': function(e, tmpl){
        adjustingEffect = true;
        updateEffect(this.name, e, e.originalEvent.clientX);
    },
    'mouseup': function(e, tmpl){
        adjustingEffect = false;
    },
    'click .magic-button': function(e, tmpl){

        if( isLeapMotionActivated(Session.get('selectedGroup')) ){
            Jam.update({
                _id:Session.get('jamId')
            },{
                $pull:{
                    leapTargets : Session.get('selectedGroup')
                }
            });
        }else{
            Jam.update({
                _id:Session.get('jamId')
            },{
                $push:{
                    leapTargets : Session.get('selectedGroup')
                }
            });
        }

    }
});

Template.JamTablet.created = function(){
    Tracker.autorun(function () {
        if(Session.get('jamId')){
            Meteor.subscribe('track-groups', Session.get('jamId'));
        }
    });
};