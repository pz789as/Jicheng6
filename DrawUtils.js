/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
} from 'react-native';

import Dimensions from 'Dimensions';
global.ScreenWidth = global.ScreenWidth || Dimensions.get('window').width;
global.ScreenHeight = global.ScreenHeight || Dimensions.get('window').height;
global.curWidth = global.curWidth || Math.min(ScreenWidth, ScreenHeight);
global.scaleWidth = global.scaleWidth || curWidth / 400;
global.minUnit = global.minUnit || ScreenWidth / 100;
global.unitDisSt = global.unitDisSt || curWidth / 25;
global.unitDisMv = global.unitDisMv || curWidth / 15;

var Utils = {
  Dis: function (x, y){
    return Math.sqrt(x * x + y * y);
  },
  DisP: function(p1, p2){
    return Utils.Dis(p1.x - p2.x, p1.y - p2.y);
  },
  Lerp: function(a, b, f){
    if (f <= 0) return a;
    if (f >= 1) return b;
    return a + (b - a) * f;
  },
  LerpP: function(a, b, f){
    if (f <= 0) return a;
    if (f >= 1) return b;
    return {
      'x': Utils.Lerp(a.x, b.x, f),
      'y': Utils.Lerp(a.y, b.y, f)
    };
  },
  PathLength: function(points){
    var d=0;
    for(var i=1;i<points.length;i++){
      d += Utils.DisP(points[i-1], points[i]);
    }
    return d;
  },
  Resample: function(points, normalizedPointsCount){
    normalizedPointsCount = Math.max(3, normalizedPointsCount);
    var intervalLength = Utils.PathLength(points) / (normalizedPointsCount-1);
    var D = 0;
    var q = {x:0, y:0};
    var normalizedPoints = [];
    normalizedPoints.push(points[0]);
    var pointBuffer = [];
    pointBuffer = pointBuffer.concat(points);
    for(var i=1;i<pointBuffer.length;i++){
      var a = pointBuffer[i-1];
      var b = pointBuffer[i];
      var d = Utils.DisP(a, b);
      if ((D+d) > intervalLength){
        q = Utils.LerpP(a, b, (intervalLength - D) / d);
        normalizedPoints.push(q);
        pointBuffer.splice(i, 0, q);
        D = 0;
      }else{
        D += d;
      }
    }
    if (normalizedPoints.length == normalizedPointsCount - 1){
      normalizedPoints.push(pointBuffer[pointBuffer.length - 1]);
    }
    return normalizedPoints;
  },
  ResampleByLen: function(points, len){
    len = Math.max(2, len);
    var normalizedPointsCount = parseInt(Utils.PathLength(points) / len);
    if (normalizedPointsCount <= 0) {
      return null;
    }
    return Utils.Resample(points, normalizedPointsCount);
  },
  CountDistance: function(arg1, arg2){
    return Math.round(Math.sqrt(Math.pow(arg1.x - arg2.x, 2) + Math.pow(arg1.y - arg2.y, 2)));
  }
};

module.exports = Utils;