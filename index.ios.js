/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import DrawLayout from './DrawLayout';
import DrawLayout1 from './DrawLayout1';
import DrawLayout2 from './DrawLayout2';

// function *gen(x){
//   for(var i=0;i<3;i++){
//     yield i;
//   }
//   var y = yield x + 2;
//   return y;
// };

class Jicheng6 extends Component {
  constructor(props){
    super(props);
    this.state = {
      kind: 2,
    };
    // var g = gen(1);
    // console.log(g.next());
    // console.log(g.next());
    // console.log(g.next());
    // console.log(g.next());
    // console.log(g.next(1));
    // var a = 1;
    // var b = 1;
    // console.log(Object.is(a,b));
  }
  onChangeModule(){
    this.setState({
      kind: (this.state.kind + 1)%3, 
    });
  }
  render() {
    return (
      <View style={styles.container}>
        {this.getRender()}
      </View>
    );
  }
  getRender(){
    if (this.state.kind == 0){
      return <DrawLayout onPress={this.onChangeModule.bind(this)}/>; 
    }else if (this.state.kind == 1) {
      return <DrawLayout1 onPress={this.onChangeModule.bind(this)}/>;
    }else if (this.state.kind == 2){
      return <DrawLayout2 onPress={this.onChangeModule.bind(this)}/>;
    }
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF',
  },
});

AppRegistry.registerComponent('Jicheng6', () => Jicheng6);
