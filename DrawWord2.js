/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  StyleSheet,
  View,
  ART,
  PanResponder,
} from 'react-native';

const {
  Shape,
  Group,
  Transform,
  Surface,
  Path,
  Pattern,
  LinearGradient,
  RadialGradient,
  // Text,
  ClippingRectangle,
} = ART;

import Utils from './Utils';

let cv = {
  stop: 0,
  play: 1,
  pause: 2,
};

export default class DrawWord2 extends Component {
  constructor(props){
    super(props);
    this.wordData = [];
    this.strokeIndex = 0;
    this.strokeTime = 0;
    this.strokeWidth = props.strokeWidth ? props.strokeWidth : 2;
    this.backColor = props.backColor ? props.backColor : '#bbb';
    this.frontColor = props.frontColor ? props.frontColor : '#222';
    this.redio = props.stdWidth / 1000;//缩放
    this.perdio = this.redio * 700;//速度
    this.status = cv.stop;
    this.initData(props.data);
    this.state = {
      blnUpdate: false
    };
    this._panResponder = {};
    this.mousePosition = {};
    this._autoUpdata = setInterval(this.autoUpdate.bind(this), 1.0/60);
  }
  initData(data){
    for(var i=0;i<data.length;i++){
      var d = data[i].split(' ');
      var points = [];
      var len = 0;
      var runTime = 0;
      var p = null;
      for(var j=0;j<d.length;){
        var np = {
          x: (parseInt(d[j + 1])) * this.redio,
          y: (parseInt(d[j + 2])) * this.redio
        };
        if (this.props.blnRandom){
          np.x += Math.random() * 2;
          np.y += Math.random() * 2;
        }
        if (p != null){
          len += Utils.DisP(p, np);
        }
        p = np;
        points.push(p);
        j+=3;
      }
      runTime = len / this.perdio;
      this.wordData.push({
        'points': points,
        'len': len,
        'runTime': runTime,
        'show': false,
      });
    }
    this.status = cv.play;
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.blnUpdate,
    });
  }
  autoUpdate(){
    if (this.status == cv.play){
      if (this.strokeIndex < this.wordData.length){
        this.strokeTime++;
        if (this.strokeTime / 60 >= this.wordData[this.strokeIndex].runTime){
          this.wordData[this.strokeIndex].show = true;
          this.strokeIndex++;
          this.strokeTime = 0;
          if (this.strokeIndex == this.wordData.length){
            this.status = cv.stop;
          }
        }
        this.setUpdate();
      }
    }
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
  }
  componentWillUnmount() {
    this._autoUpdata && clearInterval(this._autoUpdata);
  }
  onStartShouldSetPanResponder(e, g){
    console.log(e.nativeEvent.target);
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      console.log(this.mousePosition);
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    this.mousePosition = {
      x: e.nativeEvent.locationX,
      y: e.nativeEvent.locationY
    };
  }
  
  render() {
    var backLine = [];
    var showLine = [];
    var showPoints = [];
    var strokeWidth = 2;
    var spi = 0;
    for(var i=0;i<this.wordData.length;i++){
      var perT = this.strokeTime / 60 / this.wordData[i].runTime;
      var perD = perT * this.wordData[i].len;
      var p = new Path();
      var phalf = new Path();
      var point = null, lpoint = null;
      for(var j=0;j<this.wordData[i].points.length;j++){
        point = {
          x: this.wordData[i].points[j].x, 
          y: this.wordData[i].points[j].y
        };
        var pp = new Path();
        pp.moveTo(point.x, point.y - 3)
            .arc(0,6,3)
            .arc(0,-6,3)
            .close();
        showPoints.push(
          <Shape key={spi} d={pp} fill={'#f00'}/>
        );
        spi++;
        if (j==0){
          p.moveTo(point.x, point.y);
          if (i == this.strokeIndex){
            phalf.moveTo(point.x, point.y);
          }
        }else {
          p.lineTo(point.x, point.y);
          if (i == this.strokeIndex && perD > 0){
            var dis = Utils.DisP(lpoint, point);
            if (perD > dis){
              phalf.lineTo(point.x, point.y);
            }else {
              var halfp = Utils.LerpP(lpoint, point, perD / dis);
              phalf.lineTo(halfp.x, halfp.y);
            }
            perD -= dis;
          }
        }
        lpoint = point;
      }
      backLine.push(
        <Shape key={i} d={p} stroke={'#bbb'} strokeWidth={strokeWidth}/>
      );
      if (i == this.strokeIndex){
        showLine.push(
          <Shape key={i} d={phalf} stroke={'#222'} strokeWidth={strokeWidth}/>
        );
      }else if (this.wordData[i].show) {
        showLine.push(
          <Shape key={i} d={p} stroke={'#222'} strokeWidth={strokeWidth}/>
        );
      }
    }
    return (
      <View style={[styles.container, this.props.style? this.props.style : {}]} {...this._panResponder.panHandlers}>
        <Surface ref={'lineView'} width={this.props.stdWidth} height={this.props.stdWidth}>
          {backLine}
          {showLine}
          {this.props.blnSp ? showPoints : null}
        </Surface>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF',
  },
});
