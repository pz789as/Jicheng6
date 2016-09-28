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

class Jicheng6 extends Component {
  constructor(props){
    super(props);
    this.state = {
      kind: 1,
    };
  }
  onChangeModule(){
    this.setState({
      kind: (this.state.kind + 1)%2, 
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
    }else{
      return <DrawLayout1 onPress={this.onChangeModule.bind(this)}/>
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
