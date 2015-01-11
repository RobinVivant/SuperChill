
Router.configure({
    layoutTemplate: 'defaultLayout' /*,
    loadingTemplate: 'loading',
    notFoundTemplate: '404'*/
});

Router.route('/', function () {
    Router.go('/oNhS39bJ44CeFquRK');
    return;
    this.render('home');
});

Router.route('jam',{
    template: 'jam',
    path: '/:jamId',
    waitOn: function () {
        Session.set("jamId", this.params.jamId);
        var that = this;
        return [
            Meteor.subscribe('jam', this.params.jamId, {
                onError: function(error){
                    Router.go('/');
                },
                onReady: function(doc){
                    Session.set("jamName", Jam.find({_id: that.params.jamId}).fetch()[0].name);
                }
            }),
            Meteor.subscribe('samples')
        ];
    },
    data: function() {
        return {jamId: this.params.jamId}
    }

});