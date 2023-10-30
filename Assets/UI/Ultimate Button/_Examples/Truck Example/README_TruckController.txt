			
			/* ------------- < OFFICIAL > ------------- */
			/* ------- < TANK & HEALER STUDIO > ------- */
			/* --------- < 2D TRUCK EXAMPLE > --------- */

The 2D Truck Example is a perfect demonstration of how easy the Ultimate
Button is to use in any project. The very first thing that was done was
we created an Ultimate Button within the scene. For information on how to
do this, please see the Ultimate Button README located in the root Ultimate
Button folder.

Also, if you are curious about any of the functionality of the Truck
Controller, please open up the TruckController.cs script and check out the
code. We made sure to comment all that happens within the code so that it is
as easy to understand as possible. So let's see what the Ultimate Button is
doing in this example.

After creating the Ultimate Button within the scene, we just assigned
a name within the Script Reference section of the Ultimate Button Inspector.
We named the button "Gas" because it was going to be used to function like a
gas pedal of a vehicle. Once we assigned the name a box appeared that had
Example Code that we could then use by Copying and Pasting the code into the
Truck Controller script. We set the dropdown option to "Get Button". This
function from the Ultimate Button will let us know if the targeted Ultimate
Button is currently being interacted with. We used this function because we
wanted the Truck to accelerate when the user was holding down on the "Gas"
button.

We placed the Example Code inside of the FixedUpdate() function so that we
could continually get the "Gas" button's state. If you look inside the
TruckController.cs script, you can see that we have the example code within
an if conditional, and when the conditional is met, we execute the code in
order to move the truck.

This worked completely fine, but we wanted to give the truck a little bit
more character. So we decided to execute a sudden "burst" of spin to the tires
as soon as the user pressed down on the button.

In order to accomplish this, we just went back to the Ultimate Button
inspector and selected the "Get Button Down" option for the Example Code.
Then inside our truck script, we pasted the example code into an if
conditional inside the Update function. Then we can execute the code to make
the truck react more visually to the users input.

The last thing needed to finish up the truck controller was to add a "reverse"
ability to the truck. We simply recreated the steps used to create our first
button for gas. We created a new button and named it "Reverse", and added in
the checks needed inside the FixedUpdate() function to make the truck go in
reverse if all the conditions were met.

If you have any questions about the code inside the Truck Controller script,
please refer to the comments inside the code which thoroughly explain all that
happens within the script.