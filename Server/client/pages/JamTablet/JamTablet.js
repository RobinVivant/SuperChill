
Session.setDefault('tracksToGroup', {});

function isLeapMotionActivated(id){
    return _.contains(Jam.findOne({_id:Session.get('jamId')}).leapTargets, id)
}

function magicAnim(elem){
    $(elem).velocity('stop').velocity({
        properties:{
            scale: [0.5, 1]
        }, options:{
            easing:'spring',
            duration: 300,
            loop:1
        }
    });
}

Template.JamTablet.helpers({
    tracks: function () {
        return JamTracks.find();
    },
    zouzous: function(){
        return Zouzous.find({jamId: Session.get("jamId")});
    },
    displayTrack: function(){
        if( !Session.get('categoryFilter') || Session.get('categoryFilter').trim().length == 0 )
            return true;
        return this.path.match(new RegExp('\/'+Session.get('categoryFilter')+'\/', 'i'));
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
        if( tracks ){
            return tracks[this._id] ? "checkedTrack" : "";
        }
    },
    ifTrackCategorySelected: function(){
        return Session.get('categoryFilter') == this.concat() ? 'trackCategorySelected' : '';
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
    },
    getMagicClass: function(){
        return isLeapMotionActivated(Session.get('selectedGroup')) ? 'magicActivated' : '';
    },
    classEffectSelected: function(effect){
        var mapping = TrackGroups.findOne({_id: Session.get('selectedGroup')}).leapGesturesMapping[effect];
        for( var i in mapping ){
            if(mapping[i] == this.name)
                return 'activeEffect';
        }
    },
    classEffectSwitchActive: function(){
        var mappings = TrackGroups.findOne({_id: Session.get('selectedGroup')}).leapGesturesMapping;
        for( var i in mappings ){
            if(_.contains(mappings[i], this.name))
                return 'switchEffectActive';
        }
    },
    trackIconPath: function(){
        var tab = this.path.split('/');
        return '/images/'+tab[tab.length-2]+'.png';
    },
    trackCategory: function(){
        return ["Bass", "Drum Single", "Drum", "SFX", "Synth", "Vox"];
    },
    trackCategoryIcon: function(){
        return '/images/'+this+'.png';
    }
});

var updateEffectTimeout;
function updateEffect(name, e, clientX){
    var percent = (clientX-55 - $(e.currentTarget).position().left) / $(e.currentTarget).width();

    if( percent > 1 || percent < 0 ){
        return;
    }

    $(e.currentTarget).find('.slider-bar').css('width', (percent*100)+'%');

    clearTimeout(updateEffectTimeout);
    updateEffectTimeout = setTimeout(function(){
        Meteor.call('updateGroupEffect', Session.get('selectedGroup'), name, percent );
    }, 200);
}

function updateEffectState(type, name) {
    var modObj = {};
    modObj['leapGesturesMapping.'+type] = name;


    var momo = {
        _id: Session.get('selectedGroup')
    };
    momo['leapGesturesMapping.'+type] = {
        $elemMatch: {
            $in: [name]
        }
    };

    if (TrackGroups.findOne(momo)) {
        TrackGroups.update({_id: Session.get('selectedGroup')}, {
            $pull: modObj
        },{
            multi: true
        });
    } else {
        TrackGroups.update({_id: Session.get('selectedGroup')}, {
            $push: modObj
        });
    }

}

var adjustingEffect = false;

Template.JamTablet.events({
    'click .leftPanel .trackItem': function(e, tmpl){

        var trs = Session.get('tracksToGroup');

        if (trs[this._id]) {
            trs[this._id] = undefined;
            if (Object.keys(trs).length === 1) {
                $('.createGroupButton').velocity('stop').velocity({
                    properties: {
                        scale: 0,
                        opacity: 0
                    }, options: {
                        easing:'spring',
                        duration: 500,
                        complete: function () {
                            $('.createGroupButton').hide();
                        }
                    }
                });
            }
            if( Session.get('selectedGroup') ) {
                Meteor.call('removeGroupTrack', Session.get('selectedGroup'), this._id );
            }
        } else {
            if (Object.keys(trs).length === 0 && !Session.get('selectedGroup')) {
                $('.createGroupButton').show();
                $('.createGroupButton').css('bottom', ($('.leftPanel').height() - 50) / 2 + 'px');
                $('.createGroupButton').css('left', $('.leftPanel').width() - 35 + 'px');
                $('.createGroupButton').velocity('stop').velocity({
                    properties: {
                        scale: [1, 0],
                        opacity: [1, 0]
                    }, options: {
                        easing:'spring',
                        duration: 500
                    }
                });
            }
            trs[this._id] = true;
            if( Session.get('selectedGroup') ) {
                TrackGroups.update({_id: Session.get('selectedGroup')},{
                    $push : {
                        tracks : this._id
                    }
                });
            }
        }



        Session.set('tracksToGroup', trs);
    },
    'click .trackCategory': function(e, tmpl){
        if( Session.get('categoryFilter') == this.concat()){
            Session.set('categoryFilter', null);
        }else{
            Session.set('categoryFilter', this.concat());
        }
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
    'click .groupName': function(e){
        if(this._id == Session.get('selectedGroup')){
            Session.set('selectedGroup', null);
            Session.set('tracksToGroup', {});
        }else{
            Session.set('selectedGroup', this._id);
            Session.set('tracksToGroup', _.object(_.map(TrackGroups.findOne({_id:this._id}).tracks, function(x){return [x, true]})) )
        }
    },
    'click .trackGroup > .fa-trash-o': function(e, tpl){
        var that = this;
        Session.set('selectedGroup', null);
        Session.set('tracksToGroup', {});
        $(e.currentTarget).parent().velocity({
            properties:{
                scale: [0, 1],
                opacity: [0, 1]
            },options:{
                easing:'spring',
                duration: 200,
                complete: function(){
                    TrackGroups.remove({_id: that._id});
                }
            }
        })
    },
    'touchmove .effect-slider': function(e, tmpl){
        if( adjustingEffect ) {
            updateEffect(this.name, e, e.originalEvent.changedTouches[0].clientX);
        }
    },
    'touchstart .effect-slider': function(e, tmpl){
        adjustingEffect = true;
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
    'touchend': function(e, tmpl){
        adjustingEffect = false;
    },
    'click .magic-button': function(e, tmpl){

        magicAnim(e.currentTarget);

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
    },
    'click .effect-switch': function(e, tmpl){

        magicAnim(e.currentTarget);

        var elem = $(e.currentTarget).parent().find('.magic-system');

        if( elem.css('display') === 'none' ){
            elem.velocity('stop').velocity({
                properties:{
                    width: ['150px', 0],
                    marginLeft: ['-162px','-12px']
                }, options:{
                    easing:'spring',
                    display: 'inline',
                    duration: 500
                }
            });
        }else{
            elem.velocity('stop').velocity('reverse',{
                easing:'easeInBack',
                duration: 100,
                complete: function(){
                    elem.css('display', 'none');
                }
            });
        }
    },
    'click .effect-x': function(e, tmpl){
        updateEffectState('x', this.name);
    },
    'click .effect-y': function(e, tmpl){
        updateEffectState('y', this.name);
    },
    'click .effect-pitch': function(e, tmpl){
        updateEffectState('pitch', this.name);
    }
});

Template.JamTablet.created = function(){
    Tracker.autorun(function () {
        if(Session.get('jamId')){
            Meteor.subscribe('track-groups', Session.get('jamId'));
        }
    });
};