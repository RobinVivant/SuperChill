
Template.loading.helpers({

});

Template.loading.created = function(){
  Meteor.defer(function(){
    $('.fa-spin').css('line-height', $(window).height()-$('.fa-spin').height()+'px');
  });
};


