
Router.configure({
    layoutTemplate: 'defaultLayout' /*,
    loadingTemplate: 'loading',
    notFoundTemplate: '404'*/
});
/*
Router.route('/', function () {

    this.render('home');
});
*/
Router.route('jam',{
    template: 'jam',
    path: '/:jamId?',
    waitOn: function () {
        Session.set("jamId", this.params.jamId);
        var that = this;
        var sub = [Meteor.subscribe('samples'), Meteor.subscribe('jamList')];
        if( this.params.jamId ){
            sub.push(Meteor.subscribe('jam', this.params.jamId, {
                onError: function(error){
                    Router.go('/');
                },
                onReady: function(doc){
                    Session.set("jamName", Jam.find({_id: that.params.jamId}).fetch()[0].name);
                }
            }));

        }
        return sub;
    }
});