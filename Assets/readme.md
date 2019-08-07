# Notes
I have found that building the processing pipeline is best sereved using the data class Frame3D
Frame3Dprocessor is where most of the image filtering is going
there is the expetion of the LUT portion that needs to be handled on a per preview basis
I think this will still need to be done with an event based biding as I was before.