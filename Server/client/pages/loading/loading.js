
Template.loading.helpers({

});

Template.loading.created = function(){
    Meteor.defer(function(){
        $('.fa-spin').css('line-height', $(window).height()+'px');
        $('body').velocity({
            properties:{
                backgroundColor: '#'+localStorage.getItem('zouzouId')
            }, options:{
                duration:'100'
            }
        });
    });
};


Template.loading.destroyed = function(){
    Meteor.defer(function(){
        $('body').velocity('reverse');
        $('body').css('overflow', 'auto');
    });
};

