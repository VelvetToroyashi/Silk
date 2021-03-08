# Terminology used among these docs:

## You may have noticed some strange terminology used in these docs, and this page will hopfully clear some confusion. :)

----------
<br>

## **Flags:**
You may have seen the term "flag", or "user flag" thrown around.

What "flags" actually are are a numerical value attatched to a user object. 

User objects are used in Silk! extensively, and are used to track certain data about members.

Current flags used and applied to members are:

- Muted
- Temp-Banned
- Warned Prior
- Kicked Prior
- Banned Prior
- Infraction Exemption
- Staff
- Escalated staff



## **What these flags mean:**
----------

### **Muted:**
The muted flag is for rather self-evident usage, really. 

If this flag is attatched to a user object, it signifies the user is currently muted.

In the event someone were to leave a server and rejoin, this flag would tell Silk! to reapply the mute role, if any is configured to begin with.

<br>

### **Temp-Banned:**
Just as the Muted flag, temp-bans are rather self-evident in their usage, albeit with a simpler use. 

Since being banned from a server...bans you, server-members cannot rejoin while their temp-mute is active, and thus this is simply a marker flag for the sake of consistency. 

This flag may be used for the cases command in the future, however.

<br>

### **Warned Prior:**
A user is given the "warned prior" flag when they're...warned. 

A user may be warned for a multitude of reasons; both automatic and manual methods of warning member exist, as Silk! has a configureable auto-mod system. 

<br>

### **Banned Prior:**
See above.

<br>

### **Infraction Exemption**:
Warnings (also referred to as infractions internally) in Silk! can be applied to any user of a guild, technically speaking. 

However, the infraction exemption flag is special in the sense that it tells Silk! to ignore any automatic infraction applications that someone without the flag would receive a warning for.

Currently, the InfractionExemption flag is only applied to user with the **Staff** or **EscalatedStaff** flag.

