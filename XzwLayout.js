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
  PanResponder,
  TouchableOpacity,
  StatusBar,
} from 'react-native';

import Dimensions from 'Dimensions';
let ScreenWidth = Dimensions.get('window').width;
global.ScreenWidth = global.ScreenWidth || ScreenWidth;
let ScreenHeight = Dimensions.get('window').height;
global.ScreenHeight = global.ScreenHeight || ScreenHeight;
console.log(ScreenWidth, ScreenHeight);

let curWidth = Math.min(ScreenWidth, ScreenHeight);
global.curWidth = global.curWidth || curWidth;
let scaleWidth = curWidth / 400;
global.scaleWidth = global.scaleWidth || scaleWidth;
let minUnit = ScreenWidth / 100;
global.minUnit = global.minUnit || minUnit;
let unitDisSt = curWidth / 25;
let unitDisMv = curWidth / 15;

import DrawWord from './DrawWord';

var data = require('./data/瞅.json');

let Dis = function(x, y){
  return Math.sqrt(x * x + y * y);
};
global.Dis = global.Dis || Dis;
let DisP = function(p1, p2){
  return Dis(p1.x - p2.x, p1.y - p2.y);
};
global.DisP = global.DisP || DisP;
let Lerp = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return a + (b - a) * f;
}
global.Lerp = global.Lerp || Lerp;
let LerpP = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return {
    'x': Lerp(a.x, b.x, f),
    'y': Lerp(a.y, b.y, f)
  };
}
global.LerpP = global.LerpP || LerpP;

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,
};

export default class XzwLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.touchLastPoint = null;
    this.state={
      blnUpdate: false,
    };

    var gesData = require('./data/Gesture.json');
    var dataInfo = require('./data/DataInfo.json');
    var drawInfo = require('./data/DrawInfo.json');
    var orderInfo = require('./data/OrderInfo.json');
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate,
    });
  }
  componentWillMount() {
    this._panResponder = PanResponder.create({
      onStartShouldSetPanResponder: this.onStartShouldSetPanResponder.bind(this),
      onMoveShouldSetPanResponder: this.onMoveShouldSetPanResponder.bind(this),
      onPanResponderGrant: this.onPanResponderGrant.bind(this),
      onPanResponderMove: this.onPanResponderMove.bind(this),
      onPanResponderRelease: this.onPanResponderRelease.bind(this),
      onPanResponderTerminate: this.onPanResponderTerminate.bind(this),
    });
  }
  componentDidMount() {
    this._autoUpdate = setInterval(this.autoUpdate.bind(this), 1/60);
  }
  componentWillUnmount() {
    this._autoUpdate && clearInterval(this._autoUpdate);
  }
  
  onStartShouldSetPanResponder(e, g){
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      var tp = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.touchLastPoint = tp;
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      var tp = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.touchLastPoint = tp;
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    console.log('touch end');
  }
  autoUpdate(){
    
  }
  lastWord(){
    console.log('last');
  }
  nextWord(){
    console.log('next');
  }
  rewriteWord(){
    console.log('rewrite');
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <View style={styles.downView}>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.lastWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              上一个
            </Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.rewriteWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              重写
            </Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.buttonStyle} onPress={this.nextWord.bind(this)}>
            <Text style={styles.buttonTextStyle}>
              下一个
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    backgroundColor: '#F5FCFF'
  },
  upView:{
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downView: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: minUnit * 5,
    width: ScreenWidth,
  },
  buttonStyle:{
    marginBottom: minUnit * 5,
  },
  buttonTextStyle:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  }
});
