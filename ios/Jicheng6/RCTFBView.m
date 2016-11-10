//
//  RCTFBView.m
//  Jicheng6
//
//  Created by guojicheng on 16/11/10.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import "RCTFBView.h"
#import <FBSDKCoreKit/FBSDKCoreKit.h>

@implementation RCTFBView

- (instancetype)init
{
  if ((self = [super init])) {
    self.loginButton = [[FBSDKLoginButton alloc]init];
    self.loginButton.delegate = self;
    self.loginButton.readPermissions = @[@"public_profile",@"email"];
  }
  return self;
}

-(void)layoutSubviews
{
  [super layoutSubviews];
  [self.loginButton removeFromSuperview];
  self.loginButton.frame = self.bounds;
  [self insertSubview:self.loginButton atIndex:0];
  [self.loginButton setNeedsLayout];
}

- (void)loginButton:(FBSDKLoginButton *)loginButton didCompleteWithResult:(FBSDKLoginManagerLoginResult *)result error:(NSError *)error
{
  if (error != NULL){
    NSLog(@"error: %@", error.description);
  }else{
    NSLog(@"%@", result.token.tokenString);
  }
}

- (void)loginButtonDidLogOut:(FBSDKLoginButton *)loginButton
{
  NSLog(@"facebook logout!");
}

@end
